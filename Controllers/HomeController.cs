using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult LoginUser()
        {
            return View("Login");
        }
        [HttpPost("userlogin")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u=>u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                else
                {
                    HttpContext.Session.SetString("CurrentUser", userInDb.Email);
                    HttpContext.Session.SetInt32("CurrentUserID", userInDb.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Login");
            }
        }        
/////////////////////////////////////////////////////////
        [HttpGet("register")]
        public IActionResult RegisterUser()
        {
            return View("Register");
        }
        [HttpPost("userregister")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u=>u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already taken");
                    return View("Register");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    dbContext.Users.Add(user);
                    HttpContext.Session.SetString("CurrentUser", user.Email);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("CurrentUserID", user.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Register");
            }
        }
////////////////////////////////////////////////////////////
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetString("CurrentUser") != null)
            {
                int? ID =  HttpContext.Session.GetInt32("CurrentUserID");
                int realId = (int) ID;
                User currentuser = dbContext.Users.FirstOrDefault(u=>u.UserId == realId); 
                ViewBag.User = currentuser;
                ViewBag.userID = realId;
                List<Wedding> AllWeddings = dbContext.Weddings.Include(w=>w.GuestList).ToList();
                

                return View("Dashboard", AllWeddings);
            }
            else
            {
                return Redirect("/");
            }
        }
/////////////////////////////////////////////////////////////
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }
/////////////////////////////////////////////////////////////
        [HttpGet("newwedding")]
        public IActionResult NewWedding()
        {
            int? ID =  HttpContext.Session.GetInt32("CurrentUserID");
            int realId = (int) ID;
            User currentuser = dbContext.Users.FirstOrDefault(u=>u.UserId == realId); 
            ViewBag.user = currentuser;
            return View("NewWedding");
        }
        [HttpPost("addwedding")]
        public IActionResult AddWedding(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                return Redirect("/dashboard");
            }
            else
            {
                return View("NewWedding");
            }
        }
//////////////////////////////////////////////////////////////
        [HttpGet("wedding/{weddingID}")]
        public IActionResult ViewWedding(int weddingID)
        {
            Wedding currentWedding = dbContext.Weddings.Include(w=>w.GuestList).ThenInclude(w=>w.User).Where(w=>w.WeddingId == weddingID).FirstOrDefault();
            ViewBag.wedding = currentWedding;
            ViewBag.user = HttpContext.Session.GetInt32("CurrentUserID");
            return View("ViewWedding");
        }

        [HttpGet("delete/{weddingID}")]
        public IActionResult DeleteWedding(int weddingID)
        {
            Wedding currentWedding = dbContext.Weddings.Where(w=>w.WeddingId == weddingID).FirstOrDefault();
            dbContext.Remove(currentWedding);
            dbContext.SaveChanges();
            return Redirect("/dashboard");
        }

        [HttpGet("rsvp/{weddingID}")]
        public IActionResult RSVP(int weddingID)
        {
            int? ID =  HttpContext.Session.GetInt32("CurrentUserID");
            int realId = (int) ID;
            Association newAssociation = new Association {WeddingId = weddingID, UserId = realId};
            dbContext.Associations.Add(newAssociation);
            dbContext.SaveChanges();
            return Redirect("/dashboard");
        }

        [HttpGet("unrsvp/{weddingID}")]
        public IActionResult UNRSVP(int weddingID)
        {
            int? ID =  HttpContext.Session.GetInt32("CurrentUserID");
            int realId = (int) ID;
            Wedding currentWedding = dbContext.Weddings.Include(w=>w.GuestList).Where(w=>w.WeddingId == weddingID).FirstOrDefault();
            Association UserToUnRSVP = currentWedding.GuestList.FirstOrDefault(u=>u.UserId == realId);
            dbContext.Associations.Remove(UserToUnRSVP);
            dbContext.SaveChanges();
            return Redirect("/dashboard");
            
        }
    }
}
