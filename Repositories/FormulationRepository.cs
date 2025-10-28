using Microsoft.EntityFrameworkCore;
using ZebraLabelPrinter.Data;
using ZebraLabelPrinter.Models;

namespace ZebraLabelPrinter.Repositories;

/// <summary>
/// Repository for managing Formulation entities
/// Provides CRUD operations and common queries for formulations
/// </summary>
public class FormulationRepository
{
    /// <summary>
    /// Get all formulations with their ingredients
    /// </summary>
    public List<Formulation> GetAll()
    {
        using var context = new AppDbContext();
        return context.Formulations
            .Include(f => f.Ingredients)
                .ThenInclude(i => i.Product)
            .OrderBy(f => f.Name)
            .ToList();
    }

    /// <summary>
    /// Get a formulation by ID with ingredients
    /// </summary>
    public Formulation? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Formulations
            .Include(f => f.Ingredients)
                .ThenInclude(i => i.Product)
            .FirstOrDefault(f => f.Id == id);
    }

    /// <summary>
    /// Add a new formulation
    /// </summary>
    public Formulation Add(Formulation formulation)
    {
        using var context = new AppDbContext();
        context.Formulations.Add(formulation);
        context.SaveChanges();
        return formulation;
    }

    /// <summary>
    /// Update an existing formulation
    /// </summary>
    public void Update(Formulation formulation)
    {
        using var context = new AppDbContext();
        context.Formulations.Update(formulation);
        context.SaveChanges();
    }

    /// <summary>
    /// Delete a formulation by ID
    /// Ingredients will be cascade deleted automatically
    /// </summary>
    public bool Delete(int id)
    {
        using var context = new AppDbContext();
        var formulation = context.Formulations.Find(id);
        if (formulation == null) return false;

        context.Formulations.Remove(formulation);
        context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Add an ingredient to a formulation
    /// </summary>
    public void AddIngredient(FormulationIngredient ingredient)
    {
        using var context = new AppDbContext();
        context.FormulationIngredients.Add(ingredient);
        context.SaveChanges();
    }

    /// <summary>
    /// Remove an ingredient from a formulation
    /// </summary>
    public void RemoveIngredient(int ingredientId)
    {
        using var context = new AppDbContext();
        var ingredient = context.FormulationIngredients.Find(ingredientId);
        if (ingredient != null)
        {
            context.FormulationIngredients.Remove(ingredient);
            context.SaveChanges();
        }
    }

    /// <summary>
    /// Update all ingredients for a formulation
    /// Removes old ingredients and adds new ones
    /// </summary>
    public void UpdateIngredients(int formulationId, List<FormulationIngredient> newIngredients)
    {
        using var context = new AppDbContext();

        // Remove old ingredients
        var oldIngredients = context.FormulationIngredients
            .Where(i => i.FormulationId == formulationId)
            .ToList();
        context.FormulationIngredients.RemoveRange(oldIngredients);

        // Add new ingredients
        foreach (var ingredient in newIngredients)
        {
            ingredient.FormulationId = formulationId;
            context.FormulationIngredients.Add(ingredient);
        }

        context.SaveChanges();
    }
}
