using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var nextRecipeId=0;
var recipes = new ConcurrentDictionary<int,Recipe>();

app.MapGet("/recipes", () => recipes);

app.MapGet("/recipes/{id}", (int id) => recipes[id]);

app.MapPost("/recipes", (RecipeDto recipeDto) =>
{
    var recipe = new Recipe(nextRecipeId++, recipeDto.Name, recipeDto.Description, recipeDto.Ingredients);
    recipes.TryAdd(recipe.Id, recipe);
    return Results.Created($"/recipes/{recipe.Id}", recipe);
});

app.MapPut("/recipes/{id}", (int id, RecipeDto recipeDto) =>
{
    var recipe = new Recipe(id, recipeDto.Name, recipeDto.Description, recipeDto.Ingredients);
    recipes[id] = recipe;
    return Results.Ok(recipe);
});

app.MapDelete("/recipes/{id}", (int id) =>
{
    recipes.TryRemove(id, out _);
    return Results.Ok();
});

//Get a list of recipes filtered by title. The filter string must be contained in the recipe's title or in the recipe's description.
app.MapGet("/recipes/search", (string filter) =>
{
    var filteredRecipes = new ConcurrentDictionary<int, Recipe>();
    foreach (var recipe in recipes)
    {
        if (recipe.Value.Name.Contains(filter) || recipe.Value.Description.Contains(filter))
        {
            filteredRecipes.TryAdd(recipe.Key, recipe.Value);
        }
    }
    return filteredRecipes;
});

//Get a list of recipes filtered by ingredient. A recipe must be included if it contains at least on ingredients that contains the given filter string.
app.MapGet("/recipes/search", (string filter) =>
{
    var filteredRecipes = new ConcurrentDictionary<int, Recipe>();
    foreach (var recipe in recipes)
    {
        foreach (var ingredient in recipe.Value.Ingredients)
        {
            if (ingredient.Name.Contains(filter) || ingredient.Unit.Contains(filter))
            {
                filteredRecipes.TryAdd(recipe.Key, recipe.Value);
            }
        }
    }
    return filteredRecipes;
});

app.Run();

record Ingredient(string Name, string Unit, double Amount);
record RecipeDto(string Name, string Description, Ingredient[] Ingredients);
record Recipe(int Id, string Name, string Description, Ingredient[] Ingredients);
