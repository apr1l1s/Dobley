using System.ComponentModel.DataAnnotations;

namespace Dobley.Domain.Core.Entities.Products;

public enum Category
{
    [Display(Name = "Молочные продукты")]
    Dairy, // Молоко, кефир, йогурты, сметана

    [Display(Name = "Сыры")]
    Cheese, // Твердые, мягкие сыры (требуют особых условий)

    [Display(Name = "Яйца")]
    Eggs,

    [Display(Name = "Сырое мясо и птица")]
    RawMeat,

    [Display(Name = "Рыба и морепродукты")]
    FishAndSeafood,

    [Display(Name = "Колбасы и деликатесы")]
    DeliAndSausages, // Ветчина, колбаса, бекон

    [Display(Name = "Овощи")]
    Vegetables,

    [Display(Name = "Фрукты и ягоды")]
    FruitsAndBerries,

    [Display(Name = "Зелень и салаты")]
    HerbsAndGreens,

    [Display(Name = "Напитки")]
    Beverages, // Соки, вода, вино, пиво

    [Display(Name = "Соусы и приправы")]
    SaucesAndCondiments, // Майонез, кетчуп, соевый соус

    [Display(Name = "Готовые блюда и остатки")]
    ReadyMealsAndLeftovers,

    [Display(Name = "Хлеб и выпечка")]
    Bakery,

    [Display(Name = "Консервы (открытые)")]
    OpenedCannedGoods,

    [Display(Name = "Детское питание")]
    BabyFood,

    [Display(Name = "Косметика и лекарства")]
    NonFood, // Маски для лица, свечи, некоторые лекарства
}