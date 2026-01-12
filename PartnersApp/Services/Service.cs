using PartnersApp.dbContext;
using PartnersApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PartnersApp.Services
{
    public class Service : IDisposable
    {
        private PartnersDBEntities _db;
        private bool _useDatabase;
        private static bool _databaseChecked;
        private static bool _databaseAvailable;

        public Service()
        {
            if (!_databaseChecked)
            {
                CheckDatabaseConnection();
                _databaseChecked = true;
            }
            
            _useDatabase = _databaseAvailable;
            if (_useDatabase)
            {
                try { _db = new PartnersDBEntities(); }
                catch { _useDatabase = false; }
            }
        }

        private void CheckDatabaseConnection()
        {
            try
            {
                using (var testDb = new PartnersDBEntities())
                {
                    testDb.Database.Connection.Open();
                    testDb.Database.SqlQuery<int>("SELECT 1").FirstOrDefault();
                    testDb.Database.Connection.Close();
                    _databaseAvailable = true;
                }
            }
            catch 
            { 
                _databaseAvailable = false;
                MessageBox.Show("База данных недоступна. Проверьте подключение.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool Authenticate(string login, string password, out bool isAdmin)
        {
            isAdmin = false;
            
            if (_useDatabase && _db != null)
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
                    if (user != null) { isAdmin = user.IsAdmin; return true; }
                }
                catch { _useDatabase = false; }
            }
            
            if (login == "admin" && password == "admin123") { isAdmin = true; return true; }
            if (login == "user" && password == "user123") return true;
            
            return false;
        }

        public List<Partner> GetPartners()
        {
            if (!_useDatabase || _db == null)
            {
                MessageBox.Show("База данных недоступна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Partner>();
            }

            try
            {
                return _db.Partners.Select(p => new Partner
                {
                    Id = p.PartnerID,
                    Name = p.PartnerName,
                    TypeId = p.PartnerTypeID,
                    TypeName = p.PartnerTypes != null ? p.PartnerTypes.TypeName : "Не указан",
                    Director = p.DirectorName ?? "",
                    Phone = p.Phone ?? "",
                    Email = p.Email ?? "",
                    Address = p.Address ?? "",
                    Rating = p.Rating
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партнеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Partner>();
            }
        }

        public void SavePartner(Partner partner)
        {
            if (partner == null)
            {
                MessageBox.Show("Данные партнера не переданы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_useDatabase || _db == null)
            {
                MessageBox.Show("База данных недоступна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            try
            {
                Partners entity;
                if (partner.Id == 0)
                {
                    entity = new Partners();
                    _db.Partners.Add(entity);
                }
                else
                {
                    entity = _db.Partners.Find(partner.Id);
                    if (entity == null)
                    {
                        MessageBox.Show("Партнер не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                entity.PartnerName = partner.Name;
                entity.PartnerTypeID = partner.TypeId;
                entity.DirectorName = partner.Director;
                entity.Phone = partner.Phone;
                entity.Email = partner.Email;
                entity.Address = partner.Address;
                entity.Rating = partner.Rating;

                _db.SaveChanges();
                MessageBox.Show("Партнер успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения партнера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeletePartner(int id)
        {
            if (!_useDatabase || _db == null)
            {
                MessageBox.Show("База данных недоступна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            try
            {
                var partner = _db.Partners.Find(id);
                if (partner != null) 
                {
                    // Сначала удаляем связанные записи продаж
                    var sales = _db.SalesHistory.Where(s => s.PartnerID == id).ToList();
                    foreach (var sale in sales)
                    {
                        _db.SalesHistory.Remove(sale);
                    }
                    
                    // Затем удаляем партнера
                    _db.Partners.Remove(partner);
                    _db.SaveChanges();
                    MessageBox.Show("Партнер успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Партнер не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления партнера: {ex.Message}\n\nВозможно, у партнера есть связанные записи в системе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Product> GetProducts()
        {
            if (!_useDatabase || _db == null)
            {
                MessageBox.Show("База данных недоступна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Product>();
            }
            
            try
            {
                return _db.Products.Select(p => new Product
                {
                    Id = p.ProductID,
                    Name = p.ProductName,
                    Price = p.Price
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Product>();
            }
        }

        public void SaveProduct(Product product)
        {
            if (product == null || !_useDatabase || _db == null) return;
            
            try
            {
                Products entity;
                if (product.Id == 0)
                {
                    entity = new Products();
                    _db.Products.Add(entity);
                }
                else
                {
                    entity = _db.Products.Find(product.Id);
                    if (entity == null) return;
                }

                entity.ProductName = product.Name;
                entity.Price = product.Price;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteProduct(int id)
        {
            if (!_useDatabase || _db == null) return;
            
            try
            {
                var product = _db.Products.Find(id);
                if (product != null) 
                {
                    _db.Products.Remove(product);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Sale> GetSales(int partnerId)
        {
            if (!_useDatabase || _db == null)
            {
                return new List<Sale>();
            }
            
            try
            {
                return _db.SalesHistory
                    .Where(sh => sh.PartnerID == partnerId)
                    .Select(sh => new Sale
                    {
                        Id = sh.SaleID,
                        PartnerId = sh.PartnerID,
                        ProductId = sh.ProductID,
                        ProductName = sh.Products != null ? sh.Products.ProductName : "Неизвестный товар",
                        Date = sh.SaleDate,
                        Quantity = sh.Quantity,
                        Total = sh.Quantity * (sh.Products != null ? sh.Products.Price : 0)
                    }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продаж: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Sale>();
            }
        }

        public void AddSale(int partnerId, int productId, int quantity)
        {
            if (!_useDatabase || _db == null) return;
            
            try
            {
                _db.SalesHistory.Add(new SalesHistory
                {
                    PartnerID = partnerId,
                    ProductID = productId,
                    Quantity = quantity,
                    SaleDate = DateTime.Now
                });
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления продажи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public decimal GetTotalSales(int partnerId)
        {
            if (!_useDatabase || _db == null) return 0;
            
            try
            {
                var result = _db.SalesHistory
                    .Where(sh => sh.PartnerID == partnerId)
                    .Sum(sh => (decimal?)(sh.Quantity * (sh.Products != null ? sh.Products.Price : 0)));
                return result ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public int CalculateDiscount(decimal totalSales) =>
            totalSales < 10000 ? 0 :
            totalSales < 50000 ? 5 :
            totalSales < 300000 ? 10 : 15;

        public void Dispose() => _db?.Dispose();
    }
}