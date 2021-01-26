using GeneralStoreAPI_Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GeneralStoreAPI_Demo.Controllers
{
    public class TransactionController : ApiController
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        //POST
        public IHttpActionResult Post(Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Your request body cannot be empty.");
            }
            if (ModelState.IsValid)
            {
                Customer customer = _context.Customers.Find(transaction.CustomerId);
                if (customer == null)
                {
                    return BadRequest("Customer not found.");
                }
                Product product = _context.Products.Find(transaction.ProductId);
                if (product == null)
                {
                    return BadRequest("Product not found.");
                }
                if (transaction.ItemCount > (product.NumberInInventory + 1))
                {
                    return BadRequest($"Not Enough { product.Name} items in stock. \n" +
                                $"Please enter a quantity less than {product.NumberInInventory + 1}");
                }
                _context.Transactions.Add(transaction);
                product.NumberInInventory -= transaction.ItemCount;
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        //GET ALL
        public IHttpActionResult Get()
        {
            List<Transaction> transactions = _context.Transactions.ToList();
            if (transactions.Count > 0)
            {
                return Ok(transactions);
            }
            return BadRequest("Your database contains no Transactions");
        }

        //GET BY Transaction ID
        public IHttpActionResult Get(int id)
        {
            Transaction transaction = _context.Transactions.Find(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        //GET By Customer Id
        [Route("api/Transaction/GetTransactionsByCustomerID/{id}")]
        public IHttpActionResult GetByCustomerId(int id)
        {
            List<Transaction> transactionsByCustomer = _context.Transactions.Where(t => t.CustomerId == id).ToList();
            if (transactionsByCustomer.Count > 0)
            {
                return Ok(transactionsByCustomer);
            }
            return BadRequest("This customer has no transactions in the database.");
        }

        //PUT
        public IHttpActionResult Put(int id, Transaction updatedTransaction)
        {
            if (ModelState.IsValid)
            {
                Transaction oldTransaction = _context.Transactions.Find(id);
                Customer newCustomer = _context.Customers.Find(updatedTransaction.CustomerId);
                Product newProduct = _context.Products.Find(updatedTransaction.ProductId);
                Product oldProduct = oldTransaction.Product;
                if (oldTransaction != null)
                {
                    if (newCustomer != null && newProduct != null)
                    {
                        oldProduct.NumberInInventory += oldTransaction.ItemCount; //return items to inventory

                        oldTransaction.CustomerId = updatedTransaction.CustomerId;
                        oldTransaction.ProductId = updatedTransaction.ProductId;
                        oldTransaction.ItemCount = updatedTransaction.ItemCount;

                        newProduct.NumberInInventory -= updatedTransaction.ItemCount; // remove new items from inventory

                        int x = _context.SaveChanges();

                        return Ok("The transaction has been updated");
                    }
                    return BadRequest("Either the new product or new customer does not exist");
                }
                return NotFound();
            }
            return InternalServerError();
        }

        //DELETE
        public IHttpActionResult Delete(int id)
        {
            Transaction transaction = _context.Transactions.Find(id);

            if (transaction == null)
            {
                return NotFound();
            }
            _context.Transactions.Remove(transaction);
            if (_context.SaveChanges() == 1)
            {
                return Ok("Transaction Deleted");
            }
            return InternalServerError();
        }
    }
}
