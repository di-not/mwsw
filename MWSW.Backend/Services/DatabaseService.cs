using MySql.Data.MySqlClient;
using MWSW.Backend.Models;
using System.Text;

namespace MWSW.Backend.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Port=3306;Database=mwsw_db;User Id=mwsw_user;Password=mwsw_password;Charset=utf8mb4;";
        
        Console.WriteLine($"Connection string: {_connectionString}");
    }

    private static string FixEncoding(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Do not touch already-correct text. Attempt re-decoding only for classic mojibake markers.
        if (!LooksLikeMojibake(text))
            return text;

        try
        {
            var bytes = Encoding.Latin1.GetBytes(text);
            var decoded = Encoding.UTF8.GetString(bytes);

            if (decoded.Contains('\uFFFD'))
                return text;

            // Accept conversion only if it actually restores Cyrillic and does not introduce extra '?' replacements.
            return ContainsCyrillic(decoded) && CountChar(decoded, '?') <= CountChar(text, '?')
                ? decoded
                : text;
        }
        catch
        {
            return text;
        }
    }

    private static bool LooksLikeMojibake(string value) =>
        value.IndexOf('\u00D0') >= 0 ||
        value.IndexOf('\u00D1') >= 0 ||
        value.IndexOf('\u00C3') >= 0 ||
        value.IndexOf('\u00C2') >= 0;

    private static bool ContainsCyrillic(string value)
    {
        foreach (var ch in value)
        {
            if (ch >= '\u0400' && ch <= '\u04FF')
                return true;
        }

        return false;
    }

    private static int CountChar(string value, char target)
    {
        var count = 0;
        foreach (var ch in value)
        {
            if (ch == target)
                count++;
        }

        return count;
    }

    #region Planets

    public async Task<List<Planet>> GetAllPlanetsAsync()
    {
        var planets = new List<Planet>();
        
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            using var setCharsetCmd = new MySqlCommand("SET CHARACTER SET 'utf8mb4'", connection);
            await setCharsetCmd.ExecuteNonQueryAsync();
            
            var query = "SELECT * FROM planets ORDER BY name";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            Console.WriteLine("Columns in planets table: " + string.Join(", ", columnNames));
            
            while (await reader.ReadAsync())
            {
                var planet = new Planet();
                
                if (columnNames.Contains("id"))
                    planet.Id = reader.GetInt32(reader.GetOrdinal("id"));
                
                if (columnNames.Contains("name") && !reader.IsDBNull(reader.GetOrdinal("name")))
                    planet.Name = FixEncoding(reader.GetString(reader.GetOrdinal("name")));
                
                if (columnNames.Contains("short_description") && !reader.IsDBNull(reader.GetOrdinal("short_description")))
                    planet.ShortDescription = FixEncoding(reader.GetString(reader.GetOrdinal("short_description")));
                
                if (columnNames.Contains("full_description") && !reader.IsDBNull(reader.GetOrdinal("full_description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("full_description")));
                else if (columnNames.Contains("description") && !reader.IsDBNull(reader.GetOrdinal("description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("description")));
                
                if (columnNames.Contains("image_url") && !reader.IsDBNull(reader.GetOrdinal("image_url")))
                    planet.ImageUrl = reader.GetString(reader.GetOrdinal("image_url"));
                
                if (columnNames.Contains("price") && !reader.IsDBNull(reader.GetOrdinal("price")))
                    planet.Price = reader.GetDecimal(reader.GetOrdinal("price"));
                
                if (columnNames.Contains("mass") && !reader.IsDBNull(reader.GetOrdinal("mass")))
                    planet.Mass = FixEncoding(reader.GetString(reader.GetOrdinal("mass")));
                
                if (columnNames.Contains("diameter") && !reader.IsDBNull(reader.GetOrdinal("diameter")))
                    planet.Diameter = FixEncoding(reader.GetString(reader.GetOrdinal("diameter")));
                
                if (columnNames.Contains("distance_from_sun") && !reader.IsDBNull(reader.GetOrdinal("distance_from_sun")))
                    planet.DistanceFromSun = FixEncoding(reader.GetString(reader.GetOrdinal("distance_from_sun")));
                
                if (columnNames.Contains("planet_type") && !reader.IsDBNull(reader.GetOrdinal("planet_type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("planet_type")));
                else if (columnNames.Contains("type") && !reader.IsDBNull(reader.GetOrdinal("type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("type")));
                
                if (columnNames.Contains("moons_count") && !reader.IsDBNull(reader.GetOrdinal("moons_count")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons_count"));
                else if (columnNames.Contains("moons") && !reader.IsDBNull(reader.GetOrdinal("moons")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons"));
                
                if (columnNames.Contains("orbital_period") && !reader.IsDBNull(reader.GetOrdinal("orbital_period")))
                    planet.OrbitalPeriod = FixEncoding(reader.GetString(reader.GetOrdinal("orbital_period")));
                
                if (columnNames.Contains("is_sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("is_sold_out"));
                else if (columnNames.Contains("sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("sold_out"));
                
                planets.Add(planet);
            }
            
            Console.WriteLine($"Successfully loaded {planets.Count} planets");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllPlanetsAsync: {ex.Message}");
            throw;
        }
        
        return planets;
    }

    public async Task<Planet?> GetPlanetByNameAsync(string name)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            var query = "SELECT * FROM planets WHERE name = @name";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            
            using var reader = await command.ExecuteReaderAsync();
            
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            
            if (await reader.ReadAsync())
            {
                var planet = new Planet();
                
                if (columnNames.Contains("id"))
                    planet.Id = reader.GetInt32(reader.GetOrdinal("id"));
                
                if (columnNames.Contains("name") && !reader.IsDBNull(reader.GetOrdinal("name")))
                    planet.Name = FixEncoding(reader.GetString(reader.GetOrdinal("name")));
                
                if (columnNames.Contains("short_description") && !reader.IsDBNull(reader.GetOrdinal("short_description")))
                    planet.ShortDescription = FixEncoding(reader.GetString(reader.GetOrdinal("short_description")));
                
                if (columnNames.Contains("full_description") && !reader.IsDBNull(reader.GetOrdinal("full_description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("full_description")));
                else if (columnNames.Contains("description") && !reader.IsDBNull(reader.GetOrdinal("description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("description")));
                
                if (columnNames.Contains("image_url") && !reader.IsDBNull(reader.GetOrdinal("image_url")))
                    planet.ImageUrl = reader.GetString(reader.GetOrdinal("image_url"));
                
                if (columnNames.Contains("price") && !reader.IsDBNull(reader.GetOrdinal("price")))
                    planet.Price = reader.GetDecimal(reader.GetOrdinal("price"));
                
                if (columnNames.Contains("mass") && !reader.IsDBNull(reader.GetOrdinal("mass")))
                    planet.Mass = FixEncoding(reader.GetString(reader.GetOrdinal("mass")));
                
                if (columnNames.Contains("diameter") && !reader.IsDBNull(reader.GetOrdinal("diameter")))
                    planet.Diameter = FixEncoding(reader.GetString(reader.GetOrdinal("diameter")));
                
                if (columnNames.Contains("distance_from_sun") && !reader.IsDBNull(reader.GetOrdinal("distance_from_sun")))
                    planet.DistanceFromSun = FixEncoding(reader.GetString(reader.GetOrdinal("distance_from_sun")));
                
                if (columnNames.Contains("planet_type") && !reader.IsDBNull(reader.GetOrdinal("planet_type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("planet_type")));
                else if (columnNames.Contains("type") && !reader.IsDBNull(reader.GetOrdinal("type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("type")));
                
                if (columnNames.Contains("moons_count") && !reader.IsDBNull(reader.GetOrdinal("moons_count")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons_count"));
                else if (columnNames.Contains("moons") && !reader.IsDBNull(reader.GetOrdinal("moons")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons"));
                
                if (columnNames.Contains("orbital_period") && !reader.IsDBNull(reader.GetOrdinal("orbital_period")))
                    planet.OrbitalPeriod = FixEncoding(reader.GetString(reader.GetOrdinal("orbital_period")));
                
                if (columnNames.Contains("is_sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("is_sold_out"));
                else if (columnNames.Contains("sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("sold_out"));
                
                return planet;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPlanetByNameAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<int> CreatePlanetAsync(Planet planet)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();

            var query = @"
                INSERT INTO planets 
                (name, short_description, description, image_url, price, mass, diameter, distance_from_sun, type, moons, orbital_period, sold_out)
                VALUES
                (@name, @shortDescription, @description, @imageUrl, @price, @mass, @diameter, @distanceFromSun, @type, @moons, @orbitalPeriod, @soldOut);
                SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", planet.Name);
            command.Parameters.AddWithValue("@shortDescription", string.IsNullOrWhiteSpace(planet.ShortDescription) ? DBNull.Value : planet.ShortDescription);
            command.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(planet.Description) ? DBNull.Value : planet.Description);
            command.Parameters.AddWithValue("@imageUrl", string.IsNullOrWhiteSpace(planet.ImageUrl) ? DBNull.Value : planet.ImageUrl);
            command.Parameters.AddWithValue("@price", planet.Price.HasValue ? planet.Price.Value : DBNull.Value);
            command.Parameters.AddWithValue("@mass", string.IsNullOrWhiteSpace(planet.Mass) ? DBNull.Value : planet.Mass);
            command.Parameters.AddWithValue("@diameter", string.IsNullOrWhiteSpace(planet.Diameter) ? DBNull.Value : planet.Diameter);
            command.Parameters.AddWithValue("@distanceFromSun", string.IsNullOrWhiteSpace(planet.DistanceFromSun) ? DBNull.Value : planet.DistanceFromSun);
            command.Parameters.AddWithValue("@type", string.IsNullOrWhiteSpace(planet.Type) ? DBNull.Value : planet.Type);
            command.Parameters.AddWithValue("@moons", planet.Moons.HasValue ? planet.Moons.Value : DBNull.Value);
            command.Parameters.AddWithValue("@orbitalPeriod", string.IsNullOrWhiteSpace(planet.OrbitalPeriod) ? DBNull.Value : planet.OrbitalPeriod);
            command.Parameters.AddWithValue("@soldOut", planet.SoldOut);

            var insertedId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(insertedId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreatePlanetAsync: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region News

    public async Task<List<News>> GetAllNewsAsync()
    {
        var newsList = new List<News>();
        
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            var query = "SELECT * FROM news ORDER BY publish_date DESC";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            
            while (await reader.ReadAsync())
            {
                var news = new News();
                
                if (columnNames.Contains("id"))
                    news.Id = reader.GetInt32(reader.GetOrdinal("id"));
                
                if (columnNames.Contains("title") && !reader.IsDBNull(reader.GetOrdinal("title")))
                    news.Title = FixEncoding(reader.GetString(reader.GetOrdinal("title")));
                
                if (columnNames.Contains("content") && !reader.IsDBNull(reader.GetOrdinal("content")))
                    news.Content = FixEncoding(reader.GetString(reader.GetOrdinal("content")));
                
                if (columnNames.Contains("excerpt") && !reader.IsDBNull(reader.GetOrdinal("excerpt")))
                    news.Excerpt = FixEncoding(reader.GetString(reader.GetOrdinal("excerpt")));
                
                if (columnNames.Contains("image_url") && !reader.IsDBNull(reader.GetOrdinal("image_url")))
                    news.ImageUrl = reader.GetString(reader.GetOrdinal("image_url"));
                
                if (columnNames.Contains("category") && !reader.IsDBNull(reader.GetOrdinal("category")))
                    news.Category = FixEncoding(reader.GetString(reader.GetOrdinal("category")));
                
                if (columnNames.Contains("author") && !reader.IsDBNull(reader.GetOrdinal("author")))
                    news.Author = FixEncoding(reader.GetString(reader.GetOrdinal("author")));
                
                if (columnNames.Contains("publish_date") && !reader.IsDBNull(reader.GetOrdinal("publish_date")))
                    news.PublishDate = reader.GetDateTime(reader.GetOrdinal("publish_date"));
                
                if (columnNames.Contains("created_at"))
                    news.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
                
                newsList.Add(news);
            }
            
            return newsList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllNewsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<News?> GetNewsByIdAsync(int id)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            var query = "SELECT * FROM news WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            
            if (await reader.ReadAsync())
            {
                var news = new News();
                
                if (columnNames.Contains("id"))
                    news.Id = reader.GetInt32(reader.GetOrdinal("id"));
                
                if (columnNames.Contains("title") && !reader.IsDBNull(reader.GetOrdinal("title")))
                    news.Title = FixEncoding(reader.GetString(reader.GetOrdinal("title")));
                
                if (columnNames.Contains("content") && !reader.IsDBNull(reader.GetOrdinal("content")))
                    news.Content = FixEncoding(reader.GetString(reader.GetOrdinal("content")));
                
                if (columnNames.Contains("excerpt") && !reader.IsDBNull(reader.GetOrdinal("excerpt")))
                    news.Excerpt = FixEncoding(reader.GetString(reader.GetOrdinal("excerpt")));
                
                if (columnNames.Contains("image_url") && !reader.IsDBNull(reader.GetOrdinal("image_url")))
                    news.ImageUrl = reader.GetString(reader.GetOrdinal("image_url"));
                
                if (columnNames.Contains("category") && !reader.IsDBNull(reader.GetOrdinal("category")))
                    news.Category = FixEncoding(reader.GetString(reader.GetOrdinal("category")));
                
                if (columnNames.Contains("author") && !reader.IsDBNull(reader.GetOrdinal("author")))
                    news.Author = FixEncoding(reader.GetString(reader.GetOrdinal("author")));
                
                if (columnNames.Contains("publish_date") && !reader.IsDBNull(reader.GetOrdinal("publish_date")))
                    news.PublishDate = reader.GetDateTime(reader.GetOrdinal("publish_date"));
                
                if (columnNames.Contains("created_at"))
                    news.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
                
                return news;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNewsByIdAsync: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Cart

    public async Task<bool> AddToCartAsync(int? userId, int planetId, int quantity = 1)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            var checkQuery = "SELECT id, quantity FROM cart WHERE user_id = @userId AND planet_id = @planetId";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@userId", userId ?? (object)DBNull.Value);
            checkCommand.Parameters.AddWithValue("@planetId", planetId);
            
            using var reader = await checkCommand.ExecuteReaderAsync();
            var exists = await reader.ReadAsync();
            int existingId = 0;
            int existingQuantity = 0;
            
            if (exists)
            {
                existingId = reader.GetInt32(reader.GetOrdinal("id"));
                existingQuantity = reader.GetInt32(reader.GetOrdinal("quantity"));
            }
            reader.Close();
            
            if (exists)
            {
                var updateQuery = "UPDATE cart SET quantity = @newQuantity WHERE id = @id";
                using var updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@id", existingId);
                updateCommand.Parameters.AddWithValue("@newQuantity", existingQuantity + quantity);
                
                await updateCommand.ExecuteNonQueryAsync();
            }
            else
            {
                var insertQuery = "INSERT INTO cart (user_id, planet_id, quantity) VALUES (@userId, @planetId, @quantity)";
                using var insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@userId", userId ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@planetId", planetId);
                insertCommand.Parameters.AddWithValue("@quantity", quantity);
                
                await insertCommand.ExecuteNonQueryAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding to cart: {ex.Message}");
            return false;
        }
    }

    public async Task<List<CartItem>> GetCartItemsAsync(int? userId)
{
    var items = new List<CartItem>();
    
    try
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Устанавливаем кодировку для соединения
        using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
        await setNamesCmd.ExecuteNonQueryAsync();
        
        string query;
        if (userId.HasValue)
        {
            query = @"
                SELECT c.id as cart_id, c.user_id, c.planet_id, c.quantity,
                       p.id as planet_id, p.name, p.short_description, p.description,
                       p.image_url, p.price, p.mass, p.diameter, p.distance_from_sun,
                       p.type, p.moons, p.orbital_period, p.sold_out
                FROM cart c
                JOIN planets p ON c.planet_id = p.id
                WHERE c.user_id = @userId";
        }
        else
        {
            query = @"
                SELECT c.id as cart_id, c.user_id, c.planet_id, c.quantity,
                       p.id as planet_id, p.name, p.short_description, p.description,
                       p.image_url, p.price, p.mass, p.diameter, p.distance_from_sun,
                       p.type, p.moons, p.orbital_period, p.sold_out
                FROM cart c
                JOIN planets p ON c.planet_id = p.id
                WHERE c.user_id IS NULL";
        }
        
        using var command = new MySqlCommand(query, connection);
        if (userId.HasValue)
        {
            command.Parameters.AddWithValue("@userId", userId.Value);
        }
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            try
            {
                var planet = new Planet
                {
                    Id = reader.GetInt32(reader.GetOrdinal("planet_id")),
                    Name = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : FixEncoding(reader.GetString(reader.GetOrdinal("name"))),
                    ShortDescription = reader.IsDBNull(reader.GetOrdinal("short_description")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("short_description"))),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("description"))),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url")),
                    Price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("price")),
                    Mass = reader.IsDBNull(reader.GetOrdinal("mass")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("mass"))),
                    Diameter = reader.IsDBNull(reader.GetOrdinal("diameter")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("diameter"))),
                    DistanceFromSun = reader.IsDBNull(reader.GetOrdinal("distance_from_sun")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("distance_from_sun"))),
                    Type = reader.IsDBNull(reader.GetOrdinal("type")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("type"))),
                    Moons = reader.IsDBNull(reader.GetOrdinal("moons")) ? 0 : reader.GetInt32(reader.GetOrdinal("moons")),
                    OrbitalPeriod = reader.IsDBNull(reader.GetOrdinal("orbital_period")) ? null : FixEncoding(reader.GetString(reader.GetOrdinal("orbital_period"))),
                    SoldOut = reader.IsDBNull(reader.GetOrdinal("sold_out")) ? false : reader.GetBoolean(reader.GetOrdinal("sold_out"))
                };
                
                var cartItem = new CartItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("cart_id")),
                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? null : reader.GetInt32(reader.GetOrdinal("user_id")),
                    PlanetId = reader.GetInt32(reader.GetOrdinal("planet_id")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    Planet = planet
                };
                
                items.Add(cartItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading cart item: {ex.Message}");
                continue;
            }
        }
        
        return items;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in GetCartItemsAsync: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}

    public async Task<bool> RemoveFromCartAsync(int cartItemId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "DELETE FROM cart WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", cartItemId);
            
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing from cart: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "UPDATE cart SET quantity = @quantity WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", cartItemId);
            command.Parameters.AddWithValue("@quantity", quantity);
            
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating cart: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Favorites

    public async Task<bool> AddToFavoritesAsync(int userId, int planetId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "INSERT INTO favorites (user_id, planet_id) VALUES (@userId, @planetId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@planetId", planetId);
            
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding to favorites: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFromFavoritesAsync(int userId, int planetId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "DELETE FROM favorites WHERE user_id = @userId AND planet_id = @planetId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@planetId", planetId);
            
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing from favorites: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsFavoriteAsync(int userId, int planetId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM favorites WHERE user_id = @userId AND planet_id = @planetId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@planetId", planetId);
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking favorite: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Planet>> GetUserFavoritesAsync(int userId)
    {
        var planets = new List<Planet>();
        
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"
                SELECT p.* FROM planets p
                INNER JOIN favorites f ON p.id = f.planet_id
                WHERE f.user_id = @userId
                ORDER BY p.name";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            
            while (await reader.ReadAsync())
            {
                var planet = new Planet();
                
                if (columnNames.Contains("id"))
                    planet.Id = reader.GetInt32(reader.GetOrdinal("id"));
                
                if (columnNames.Contains("name") && !reader.IsDBNull(reader.GetOrdinal("name")))
                    planet.Name = FixEncoding(reader.GetString(reader.GetOrdinal("name")));
                
                if (columnNames.Contains("short_description") && !reader.IsDBNull(reader.GetOrdinal("short_description")))
                    planet.ShortDescription = FixEncoding(reader.GetString(reader.GetOrdinal("short_description")));
                
                if (columnNames.Contains("full_description") && !reader.IsDBNull(reader.GetOrdinal("full_description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("full_description")));
                else if (columnNames.Contains("description") && !reader.IsDBNull(reader.GetOrdinal("description")))
                    planet.Description = FixEncoding(reader.GetString(reader.GetOrdinal("description")));
                
                if (columnNames.Contains("image_url") && !reader.IsDBNull(reader.GetOrdinal("image_url")))
                    planet.ImageUrl = reader.GetString(reader.GetOrdinal("image_url"));
                
                if (columnNames.Contains("price") && !reader.IsDBNull(reader.GetOrdinal("price")))
                    planet.Price = reader.GetDecimal(reader.GetOrdinal("price"));
                
                if (columnNames.Contains("mass") && !reader.IsDBNull(reader.GetOrdinal("mass")))
                    planet.Mass = FixEncoding(reader.GetString(reader.GetOrdinal("mass")));
                
                if (columnNames.Contains("diameter") && !reader.IsDBNull(reader.GetOrdinal("diameter")))
                    planet.Diameter = FixEncoding(reader.GetString(reader.GetOrdinal("diameter")));
                
                if (columnNames.Contains("distance_from_sun") && !reader.IsDBNull(reader.GetOrdinal("distance_from_sun")))
                    planet.DistanceFromSun = FixEncoding(reader.GetString(reader.GetOrdinal("distance_from_sun")));
                
                if (columnNames.Contains("planet_type") && !reader.IsDBNull(reader.GetOrdinal("planet_type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("planet_type")));
                else if (columnNames.Contains("type") && !reader.IsDBNull(reader.GetOrdinal("type")))
                    planet.Type = FixEncoding(reader.GetString(reader.GetOrdinal("type")));
                
                if (columnNames.Contains("moons_count") && !reader.IsDBNull(reader.GetOrdinal("moons_count")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons_count"));
                else if (columnNames.Contains("moons") && !reader.IsDBNull(reader.GetOrdinal("moons")))
                    planet.Moons = reader.GetInt32(reader.GetOrdinal("moons"));
                
                if (columnNames.Contains("orbital_period") && !reader.IsDBNull(reader.GetOrdinal("orbital_period")))
                    planet.OrbitalPeriod = FixEncoding(reader.GetString(reader.GetOrdinal("orbital_period")));
                
                if (columnNames.Contains("is_sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("is_sold_out"));
                else if (columnNames.Contains("sold_out"))
                    planet.SoldOut = reader.GetBoolean(reader.GetOrdinal("sold_out"));
                
                planets.Add(planet);
            }
            
            return planets;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserFavoritesAsync: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Users

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
             
            var query = "SELECT * FROM users WHERE email = @email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    FullName = reader.GetString(reader.GetOrdinal("full_name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                    BirthDate = reader.IsDBNull(reader.GetOrdinal("birth_date")) ? null : reader.GetDateTime(reader.GetOrdinal("birth_date")),
                    RegistrationDate = reader.GetDateTime(reader.GetOrdinal("registration_date")),
                    AvatarColor = reader.IsDBNull(reader.GetOrdinal("avatar_color")) ? "#3498db" : reader.GetString(reader.GetOrdinal("avatar_color"))
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserByEmailAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> CreateUserAsync(User user, string passwordHash)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
            await setNamesCmd.ExecuteNonQueryAsync();
            
            var query = @"
                INSERT INTO users (full_name, email, password_hash, phone, birth_date, avatar_color, registration_date) 
                VALUES (@fullName, @email, @passwordHash, @phone, @birthDate, @avatarColor, @registrationDate)";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@fullName", user.FullName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(user.Phone) ? (object)DBNull.Value : user.Phone);
            command.Parameters.AddWithValue("@birthDate", user.BirthDate.HasValue ? (object)user.BirthDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@avatarColor", string.IsNullOrEmpty(user.AvatarColor) ? "#3498db" : user.AvatarColor);
            command.Parameters.AddWithValue("@registrationDate", DateTime.Now);
            
            var result = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"CreateUserAsync result: {result}");
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateUserAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    #endregion
}
