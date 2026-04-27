using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using HeroesApi.Models;
using HeroesApi.Data;

namespace HeroesApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class HeroesController : ControllerBase {
    [HttpGet]
    public ActionResult<List<Hero>> GetAll() {
        return Ok(HeroesStore.Heroes);
    }
    [HttpGet("{id}")]
    public ActionResult<Hero> GetById(int id) {
        var hero = HeroesStore.Heroes.FirstOrDefault(h => h.Id == id);
        if (hero is null) {
            return NotFound(new { message = $"Герой с id={id} не найден" });
        }
        return Ok(hero);
    }
    [HttpGet("demo")]
    public ActionResult GetDemo() {
        var hero = HeroesStore.Heroes.First();
        var defaultOptions = new JsonSerializerOptions {
            WriteIndented = true
        };
        var ourOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        return Ok(new {
            withDefaultSettings = JsonSerializer.Deserialize<object>(
                JsonSerializer.Serialize(hero, defaultOptions), defaultOptions),
            withOurSettings = JsonSerializer.Deserialize<object>(
                JsonSerializer.Serialize(hero, ourOptions), ourOptions),
            note = "Сравните имена полей и значение universe в двух вариантах"
        });
    }
    [HttpGet("serialize")]
    public ActionResult GetSerializationDemo() {
        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var hero = new Hero {
            Id = 99,
            Name = "Тестовый герой",
            RealName = "Студент",
            Universe = Universe.Marvel,
            PowerLevel = 50,
            Powers = new() { "программирование", "дебаггинг" },
            Weapon = new() { Name = "Клавиатура", IsRanged = false },
            InternalNotes = "Это поле не попадёт в JSON!"
        };
        string serialized = JsonSerializer.Serialize(hero, options);
        var deserialized = JsonSerializer.Deserialize<Hero>(serialized, options);
        return Ok(new {
            serializedJson = serialized,
            deserializedObject = deserialized,
            internalNotesAfterDeserialize = deserialized?.InternalNotes ?? "null - поле будет проигнорировано"
        });
    }
    [HttpGet]
    public ActionResult<List<Hero>> GetAll([FromQuery] string? universe = null) {
        var heroes = HeroesStore.Heroes.AsEnumerable();
        if (!string.IsNullOrEmpty(universe)) {
            if (universe.Equals("Marvel", StringComparison.OrdinalIgnoreCase)) {
                heroes = heroes.Where(h => h.Universe == Universe.Marvel);
            }
            else if (universe.Equals("DC", StringComparison.OrdinalIgnoreCase)) {
                heroes = heroes.Where(h => h.Universe == Universe.DC);
            }
        }
        return Ok(heroes.ToList());
    }
    [HttpGet("search")]
    public ActionResult<List<Hero>> Search([FromQuery] string name) {
        if (string.IsNullOrEmpty(name)) {
            return BadRequest(new { message = "Параметр 'name' обязателен" });
        }
        var heroes = HeroesStore.Heroes
            .Where(h => h.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (!heroes.Any()) {
            return NotFound(new { message = $"Герои с именем, содержащим '{name}', не найдены" });
        }
        return Ok(heroes);
    }
}