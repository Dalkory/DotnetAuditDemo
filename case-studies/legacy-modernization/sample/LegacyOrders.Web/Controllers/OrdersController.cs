using System.Linq;
using System.Web.Mvc;
using LegacyOrders.Web.Data;

namespace LegacyOrders.Web.Controllers
{
    public sealed class OrdersController : Controller
    {
        private readonly OrdersContext _db = new OrdersContext();

        public ActionResult Index()
        {
            var orders = _db.Orders
                .OrderByDescending(order => order.Id)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public ActionResult Create(string externalReference, decimal amount)
        {
            _db.Orders.Add(new Order
            {
                ExternalReference = externalReference,
                Amount = amount
            });
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
