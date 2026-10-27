using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.VisualBasic;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Data;
using Shop.Catalog.Api.Entities;

namespace Shop.Catalog.Api.Seed;

public static class CatalogSeeder
{
    public static async Task SeedCatalogAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CatalogSeeder");

        if (await db.Categories.AnyAsync())
        {
            logger.LogInformation("Catalog Already seeded, skipping");
            return;
        }

        var electronics = NewCategory("Электроника");
        var phones = NewCategory("Смартфоны", electronics);
        var laptops = NewCategory("Ноутбуки", electronics);
        var headphones = NewCategory("Наушники", electronics);
        var home = NewCategory("Дом и кухня");
        var cookware = NewCategory("Посуда", home);
        var sport = NewCategory("Спорт");
        var fitness = NewCategory("Фитнес", sport);
        
        db.Categories.AddRange(electronics, phones, laptops, headphones, home, cookware, sport,fitness);

        var random = new Random(42);
        var products = new List<Product>();

        AddProducts(products, phones, random, 9_000, 140_000,
        [
            "Apple iPhone 15", "Apple iPhone 15 Pro", "Apple iPhone 14", "Samsung Galaxy S24",
            "Samsung Galaxy S24 Ultra", "Samsung Galaxy A55", "Samsung Galaxy A35", "Xiaomi 14",
            "Xiaomi Redmi Note 13", "Xiaomi Redmi 13C", "Google Pixel 8", "Google Pixel 8a",
            "OnePlus 12", "Honor 200", "Honor Magic6 Pro", "Realme 12 Pro", "Nothing Phone 2a",
            "Motorola Edge 50",
        ]);

        AddProducts(products, laptops, random, 40_000, 250_000,
        [
            "Apple MacBook Air 13 M3", "Apple MacBook Air 15 M3", "Apple MacBook Pro 14 M3",
            "Lenovo ThinkPad X1 Carbon", "Lenovo IdeaPad Slim 5", "Lenovo Legion 5", "ASUS Zenbook 14",
            "ASUS Vivobook 16", "ASUS ROG Strix G16", "HP Pavilion 15", "HP EliteBook 840",
            "HP Victus 16", "Dell XPS 13", "Dell Inspiron 15", "Acer Swift Go 14", "Acer Nitro V 15",
            "Huawei MateBook D 16", "MSI Modern 14",
        ]);

        AddProducts(products, headphones, random, 2_000, 45_000,
        [
            "Apple AirPods Pro 2", "Apple AirPods 3", "Sony WH-1000XM5", "Sony WF-1000XM5",
            "Sony WH-CH720N", "Bose QuietComfort Ultra", "Bose QuietComfort Earbuds II",
            "JBL Tune 770NC", "JBL Live Pro 2", "Sennheiser Momentum 4", "Sennheiser Accentum",
            "Samsung Galaxy Buds2 Pro", "Samsung Galaxy Buds FE", "Xiaomi Redmi Buds 5 Pro",
            "Marshall Major IV", "Marshall Motif II", "Jabra Elite 8 Active", "Anker Soundcore Liberty 4",
        ]);

        AddProducts(products, cookware, random, 500, 15_000,
        [
            "Сковорода Tefal Ingenio 26 см", "Сковорода Tefal Excellence 28 см", "Кастрюля Rondell 3 л",
            "Кастрюля Rondell 5 л", "Набор кастрюль Fissman 6 предметов", "Сотейник Нева Металл 26 см",
            "Чайник заварочный Bodum 1 л", "Набор ножей Victorinox 3 шт", "Нож шеф Samura 20 см",
            "Разделочная доска бамбуковая", "Форма для запекания стеклянная 2 л",
            "Набор тарелок Luminarc 18 предметов", "Кружка керамическая 350 мл", "Набор бокалов Bohemia 6 шт",
            "Термос Thermos 1 л", "Ланч-бокс стеклянный", "Турка медная 400 мл", "Вок чугунный 30 см",
        ]);

        AddProducts(products, fitness, random, 300, 35_000,
        [
            "Гантели разборные 2×10 кг", "Гиря 16 кг", "Коврик для йоги 6 мм", "Фитнес-резинки набор 5 шт",
            "Скакалка скоростная", "Ролик для пресса", "Турник настенный", "Эспандер кистевой", "Степ-платформа",
            "Мяч гимнастический 65 см", "Фитнес-браслет Xiaomi Smart Band 8", "Пульсометр Polar H10",
            "Бутылка для воды 750 мл", "Массажный ролл", "Утяжелители 2×1 кг", "Блок для йоги",
            "Перчатки для фитнеса", "Велотренажёр магнитный",
        ]);

        db.Products.AddRange(products);
        logger.LogInformation("Catalog seeded: {Categories} categories, {Products} products", 8, products.Count);

    }


    private static Category NewCategory(string name, Category? parent = null) => new()
    {
        Name = name,
        Slug = SlugGenerator.Generate(name),
        Parent = parent,
    };

    private static void AddProducts(
        List<Product> target, Category category, Random random,
        int minPrice, int maxPrice, string[] names)
    {
        foreach(var name in names)
        {
            target.Add(new Product
            {
                Name = name,
                Slug = SlugGenerator.Generate(name),
                Description = $"{name} - демо описание товара для каталога.",
                Price = random.Next(minPrice/100, maxPrice/100) * 100-10,
                Stock = random.Next(0, 50),
                Category = category,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(0,90))
            });
        }
    }

}