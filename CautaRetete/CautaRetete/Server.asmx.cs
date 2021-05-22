using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace CautaRetete
{
    /// <summary>
    /// Summary description for Server
    /// </summary>
    //[WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Server : WebService
    {

        private Models.recipesEntities recipes = new Models.recipesEntities();
        private Models.ingredientsEntities ingredients = new Models.ingredientsEntities();
        private Models.spicesEntities spices = new Models.spicesEntities();
        private Models.recipeIngredientsEntities recipeIngredients = new Models.recipeIngredientsEntities();
        private Models.recipeSpicesEntities recipeSpices = new Models.recipeSpicesEntities();
        private Models.usersEntities users = new Models.usersEntities();

        // Users methods

        [WebMethod]
        public bool ExistsUser(string username)
        {
            var user = users.Users.Where(u => u.username == username);
            if(user.Count() > 0)
            {
                return true;
            }
            return false;
        }

        [WebMethod]
        public bool PostUser(string username, string password)
        {
            if (!ExistsUser(username))
            {
                var user = new Models.Users()
                {
                    username = username,
                    password = password
                };
                users.Users.Add(user);
                users.SaveChanges();
                return true;
            }
            return false;
        }

        // Recipes methods
        [WebMethod]
        public List<Models.Recipes> GetRecipes()
        {
            return recipes.Recipes.ToList();
        }

        [WebMethod]
        public Models.Recipes GetRecipeById(int id)
        {
            return recipes.Recipes.Find(id);
        }

        [WebMethod]
        public Models.Recipes GetRecipeIdByName(string name)
        {
            var x = recipes.Recipes.Where(r => r.Name == name);
            if (x.Count() > 0)
                return x.First();
            return null;
        }

        [WebMethod]
        public void PostRecipe(Models.Recipes recipe, List<Models.Ingredients> ingredientList, List<Models.Spices> spiceList)
        {
            var has = recipes.Recipes.Find(recipe.Id);
            if (has == null)
            {
                recipes.Recipes.Add(recipe);
            }
            foreach (var i in ingredientList)
            {
                var x = ingredients.Ingredients.Find(i.Id);
                if (x == null)
                {
                    ingredients.Ingredients.Add(i);
                }

                var ri = new Models.RecipeIngredients
                {
                    RecipeId = recipe.Id,
                    IngredientsId = i.Id
                };
                var y = recipeIngredients.RecipeIngredients.Find(ri);
                if (y == null)
                {
                    recipeIngredients.RecipeIngredients.Add(ri);
                }
            }
            foreach (var s in spiceList)
            {
                var x = spices.Spices.Find(s.Id);
                if (x == null)
                {
                    spices.Spices.Add(s);
                }

                var rs = new Models.RecipeSpices
                {
                    RecipeId = recipe.Id,
                    SpiceId = s.Id
                };
                var y = recipeIngredients.RecipeIngredients.Find(rs);
                if (y == null)
                {
                    recipeSpices.RecipeSpices.Add(rs);
                }
            }
            recipes.SaveChanges();
            ingredients.SaveChanges();
            spices.SaveChanges();
            recipeIngredients.SaveChanges();
            recipeSpices.SaveChanges();
        }

        [WebMethod]
        public List<Models.Ingredients> GetRecipeIngredients(int id)
        {
            var x = recipeIngredients.RecipeIngredients.Join(
                ingredients.Ingredients,
                ri => ri.IngredientsId,
                i => i.Id,
                (ri, i) => new { ri, i })
                .Where(r => r.ri.RecipeId == id);
            return (List<Models.Ingredients>)x.Select(r => r.i);
        }

        [WebMethod]
        public List<Models.Spices> GetRecipeSpices(int id)
        {
            var x = recipeSpices.RecipeSpices.Join(
                spices.Spices,
                rs => rs.SpiceId,
                s => s.Id,
                (rs, s) => new { rs, s })
                .Where(r => r.rs.RecipeId == id);
            return (List<Models.Spices>)x.Select(r => r.s);
        }

        [WebMethod]
        public void PostRecipeString(string name, string description, List<string> ingredientList, List<string> spiceList)
        {
            var r = new Models.Recipes
            {
                Name = name,
                Description = description
            };
            if (!ExistsRecipe(name))
            {
                recipes.Recipes.Add(r);
            }
            recipes.SaveChanges();
            foreach (var i in ingredientList)
            {
                if (!ExistsIngredient(i))
                {
                    ingredients.Ingredients.Add(
                        new Models.Ingredients
                        {
                            Name = i
                        });
                }
                ingredients.SaveChanges();
                var recipeId = GetRecipeIdByName(name);
                var ingredientId = GetIngredientIdByName(i);
                if (recipeId != null && ingredientId != null)
                {
                    if (!ExistsRecipeIngredient(recipeId.Id, ingredientId.Id))
                    {
                        recipeIngredients.RecipeIngredients.Add(
                            new Models.RecipeIngredients
                            {
                                RecipeId = recipeId.Id,
                                IngredientsId = ingredientId.Id
                            });
                    }
                }

            }
            foreach (var s in spiceList)
            {
                if (!ExistsSpice(s))
                {
                    spices.Spices.Add(
                        new Models.Spices
                        {
                            Name = s
                        });
                }
                spices.SaveChanges();
                var recipeId = GetRecipeIdByName(name);
                var spiceId = GetSpiceIdByName(s);
                if (recipeId != null && spiceId != null)
                {
                    if (!ExistsRecipeSpice(recipeId.Id, spiceId.Id))
                    {
                        recipeSpices.RecipeSpices.Add(
                            new Models.RecipeSpices
                            {
                                RecipeId = recipeId.Id,
                                SpiceId = spiceId.Id
                            });
                    }
                }
            }


            recipeIngredients.SaveChanges();
            recipeSpices.SaveChanges();
        }

        [WebMethod]
        public void PostRecipeOnlyString(string name, string description)
        {
            recipes.Recipes.Add(
                new Models.Recipes
                {
                    Name = name,
                    Description = description
                });
            recipes.SaveChanges();
        }

        [WebMethod]
        public void UpdateRecipe(int id, string recipeName, string recipeDescription)
        {
            var r = recipes.Recipes.Find(id);
            r.Name = recipeName;
            r.Description = recipeDescription;
            recipes.SaveChanges();
        }

        /*
        [WebMethod]
        public void UpdateRecipe(int id, string recipeName, string recipeDescription, List<string> ingredientList, List<string> spiceList)
        {
            var r = recipes.Recipes.Find(id);
            r.Name = recipeName;
            r.Description = recipeDescription;
            recipes.SaveChanges();
            foreach (var i in ingredientList)
            {
                if (!ExistsIngredient(i))
                {
                    ingredients.Ingredients.Add(
                        new Models.Ingredients
                        {
                            Name = i
                        });
                }
                ingredients.SaveChanges();
                var ingredient = GetIngredientIdByName(i);
                if (ingredient != null)
                {
                    if (!ExistsRecipeIngredient(id, ingredient.Id))
                    {
                        recipeIngredients.RecipeIngredients.Add(
                            new Models.RecipeIngredients
                            {
                                RecipeId = id,
                                IngredientsId = ingredient.Id
                            });
                    }
                }

            }
            foreach (var s in spiceList)
            {
                if (!ExistsSpice(s))
                {
                    spices.Spices.Add(
                        new Models.Spices
                        {
                            Name = s
                        });
                }
                spices.SaveChanges();
                var spiceId = GetSpiceIdByName(s);
                if (spiceId != null)
                {
                    if (!ExistsRecipeSpice(id, spiceId.Id))
                    {
                        recipeSpices.RecipeSpices.Add(
                            new Models.RecipeSpices
                            {
                                RecipeId = id,
                                SpiceId = spiceId.Id
                            });
                    }
                }
            }
            recipeIngredients.SaveChanges();
            recipeSpices.SaveChanges();
        }
        */

        [WebMethod]
        public void DeleteRecipe(int id)
        {
            recipes.Recipes.Remove(GetRecipeById(id));
            var i = recipeIngredients.RecipeIngredients.Where(ri => ri.IngredientsId == id).ToArray();
            recipeIngredients.RecipeIngredients.RemoveRange(i);
            var s = recipeSpices.RecipeSpices.Where(rs => rs.RecipeId == id).ToArray();
            recipeSpices.RecipeSpices.RemoveRange(s);

            recipes.SaveChanges();
            recipeIngredients.SaveChanges();
            recipeSpices.SaveChanges();
        }

        // Ingredients methods
        [WebMethod]
        public List<Models.Ingredients> GetIngredients()
        {
            return ingredients.Ingredients.ToList();
        }

        [WebMethod]
        public Models.Ingredients GetIngredientById(int id)
        {
            return ingredients.Ingredients.Find(id);
        }

        [WebMethod]
        public Models.Ingredients GetIngredientIdByName(string name)
        {
            var x = ingredients.Ingredients.Where(i => i.Name == name);
            if (x.Count() > 0)
                return x.First();
            return null;
        }

        [WebMethod]
        public List<Models.Ingredients> GetIngredientsByRecipeId(int id)
        {
            var i = recipeIngredients.RecipeIngredients.Where(ri => ri.RecipeId == id);
            List<Models.Ingredients> x = new List<Models.Ingredients>();
            foreach (var ii in i)
            {
                x.Add(GetIngredientById(ii.IngredientsId));
            }
            return x;
        }

        [WebMethod]
        public void PostIngredient(Models.Ingredients ingredient)
        {
            ingredients.Ingredients.Add(ingredient);
            ingredients.SaveChanges();
        }

        [WebMethod]
        public void PostIngredientString(string ingredientName)
        {
            ingredients.Ingredients.Add(
                new Models.Ingredients
                {
                    Name = ingredientName
                });
            ingredients.SaveChanges();
        }

        [WebMethod]
        public void UpdateIngredient(int id, string ingredientName)
        {
            ingredients.Ingredients.Find(id).Name = ingredientName;
            ingredients.SaveChanges();
        }

        [WebMethod]
        public void DeleteIngredient(int id)
        {
            ingredients.Ingredients.Remove(GetIngredientById(id));
            ingredients.SaveChanges();
        }


        // Spices methods
        [WebMethod]
        public List<Models.Spices> GetSpices()
        {
            return spices.Spices.ToList();
        }

        [WebMethod]
        public Models.Spices GetSpiceById(int id)
        {
            return spices.Spices.Find(id);
        }
        [WebMethod]
        public Models.Spices GetSpiceIdByName(string name)
        {
            var x = spices.Spices.Where(s => s.Name == name);
            if (x.Count() > 0)
                return x.First();
            return null;
        }

        [WebMethod]
        public List<Models.Spices> GetSpicesByRecipeId(int id)
        {
            var i = recipeSpices.RecipeSpices.Where(rs => rs.RecipeId == id);
            List<Models.Spices> x = new List<Models.Spices>();
            foreach (var ii in i)
            {
                x.Add(GetSpiceById(ii.SpiceId));
            }
            return x;
        }

        [WebMethod]
        public void PostSpice(Models.Spices spice)
        {
            spices.Spices.Add(spice);
            spices.SaveChanges();
        }

        [WebMethod]
        public void PostSpiceString(string spiceName)
        {
            spices.Spices.Add(
                new Models.Spices
                {
                    Name = spiceName
                });
            spices.SaveChanges();
        }

        [WebMethod]
        public void UpdateSpice(int id, string spiceName)
        {
            spices.Spices.Find(id).Name = spiceName;
            spices.SaveChanges();
        }

        [WebMethod]
        public void DeleteSpice(int id)
        {
            spices.Spices.Remove(GetSpiceById(id));
            spices.SaveChanges();
        }

        [WebMethod]
        public bool ExistsIngredient(string name)
        {
            if (GetIngredientIdByName(name) == null)
                return false;
            return true;
        }

        [WebMethod]
        public bool ExistsSpice(string name)
        {
            if (GetSpiceIdByName(name) == null)
                return false;
            return true;
        }

        [WebMethod]
        public bool ExistsRecipe(string name)
        {
            if (GetRecipeIdByName(name) == null)
                return false;
            return true;
        }


        [WebMethod]
        public bool ExistsRecipeIngredient(int recipeId, int ingredientId)
        {
            var a = recipeIngredients.RecipeIngredients.Where(x => x.RecipeId == recipeId && x.IngredientsId == ingredientId);
            if (a.Count() > 0)
                return true;
            return false;
        }

        [WebMethod]
        public bool ExistsRecipeSpice(int recipeId, int spiceId)
        {
            var a = recipeSpices.RecipeSpices.Where(x => x.RecipeId == recipeId && x.SpiceId == spiceId);
            if (a.Count() > 0)
                return true;
            return false;
        }

        [WebMethod]
        public void DeleteRecipeIngredient(int recipeId, int ingredientId)
        {
            var a = recipeIngredients.RecipeIngredients.Where(x => x.RecipeId == recipeId && x.IngredientsId == ingredientId);
            if (a.Count() > 0)
            {
                recipeIngredients.RecipeIngredients.Remove(a.FirstOrDefault());
                recipeIngredients.SaveChanges();
            }
        }

        [WebMethod]
        public void DeleteRecipeSpice(int recipeId, int spiceId)
        {
            var a = recipeSpices.RecipeSpices.Where(x => x.RecipeId == recipeId && x.SpiceId == spiceId);
            if (a.Count() > 0)
            {
                recipeSpices.RecipeSpices.Remove(a.FirstOrDefault());
                recipeSpices.SaveChanges();
            }
        }
    }
}
