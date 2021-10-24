using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeBox.Models;

namespace RecipeBox.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly RecipeBoxContext _db;

        private readonly UserManager<ApplicationUser> _userManager;

        public RecipesController(
            UserManager<ApplicationUser> userManager,
            RecipeBoxContext db
        )
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<ActionResult> Index(string searchString)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);

            IQueryable<Recipe> userRecipes =
                _db
                    .Recipes
                    .Where(entry => entry.User.Id == currentUser.Id)
                    .OrderByDescending(rate => rate.Rate);
            if (!string.IsNullOrEmpty(searchString))
            {
                userRecipes =
                    userRecipes
                        .Where(rate => rate.Description.Contains(searchString));
            }
            return View(userRecipes.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.CategoryId =
                new SelectList(_db.Categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Recipe recipe, int CategoryId)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            recipe.User = currentUser;
            _db.Recipes.Add (recipe);
            _db.SaveChanges();
            if (CategoryId != 0)
            {
                _db
                    .CategoryRecipe
                    .Add(new CategoryRecipe()
                    { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var thisRecipe =
                _db
                    .Recipes
                    .Include(recipe => recipe.JoinEntities)
                    .ThenInclude(join => join.Category)
                    .FirstOrDefault(Recipe => Recipe.RecipeId == id);
            return View(thisRecipe);
        }

        public ActionResult Edit(int id)
        {
            var thisRecipe =
                _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            ViewBag.CategoryId =
                new SelectList(_db.Categories, "CategoryId", "Name");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult Edit(Recipe recipe, int CategoryId)
        {
            if (CategoryId != 0)
            {
                _db
                    .CategoryRecipe
                    .Add(new CategoryRecipe()
                    { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
            }
            _db.Entry(recipe).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddCategory(int id)
        {
            var thisRecipe =
                _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            ViewBag.CategoryId =
                new SelectList(_db.Categories, "CategoryId", "Name");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult AddCategory(Recipe recipe, int CategoryId)
        {
            if (CategoryId != 0)
            {
                _db
                    .CategoryRecipe
                    .Add(new CategoryRecipe()
                    { CategoryId = CategoryId, RecipeId = recipe.RecipeId });
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var thisRecipe =
                _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            return View(thisRecipe);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var thisRecipe =
                _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
            _db.Recipes.Remove (thisRecipe);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteCategory(int joinId)
        {
            var joinEntry =
                _db
                    .CategoryRecipe
                    .FirstOrDefault(entry => entry.CategoryRecipeId == joinId);
            _db.CategoryRecipe.Remove (joinEntry);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
