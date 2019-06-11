using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            //calculate total and save it to viewbag
            decimal total = 0m;
            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;
            //return view with model
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //init cartVM
            CartVM model = new CartVM();
            //init quantity
            int qty = 0;
            //init price
            decimal price = 0m;
            //check for cart session
            if (Session["cart"] != null)
            {
                //get total qty and price
                var list = (List<CartVM>) Session["cart"];
                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Price * item.Quantity;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }




            //return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //init cartvm list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //init cartvm
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //get the product
                ProductDTO product = db.Products.Find(id);
                //check if the product is already in cat
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                //if not add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName

                    });
                }
                else
                {
                    //if it is increment
                    productInCart.Quantity++;
                }


            }


            //get total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //save cart back to session
            Session["cart"] = cart;
            //return partial view with model
            return PartialView(model);


        }

        public JsonResult IncrementProduct(int productId)
        {
            //init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get cartvm from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //increment qty
                model.Quantity++;

                //store needed data
                var result = new {qty = model.Quantity, Price = model.Price};

                //return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }





        }

        public JsonResult DecrementProduct(int productId)
        {
            //init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get cartvm from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //increment qty
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //store needed data
                var result = new {qty = model.Quantity, Price = model.Price};

                //return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }





        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get cartvm from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);


                //remove from list

                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {

            //get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            //get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                //init orderdto
                OrderDTO orderDTO= new OrderDTO();
                //get userid
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;
                //add to orderdto and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt=DateTime.Now;

                db.Order.Add(orderDTO);

                db.SaveChanges();
                //get inserted in
                orderId = orderDTO.OrderId;
                //init orderdetailsdto
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //add to orderdetailsdto
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.orderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }


            //email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("780fa685a2fec3", "1508208b592765"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order",
                "You have a new order. Order number is " + orderId);
            //reset session
            Session["cart"] = null;
        }
    }
}