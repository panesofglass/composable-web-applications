using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ContactManager.Controllers.Apis;
using ContactManager.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ContactManager.Tests.Controllers
{
    [TestClass]
    public class ContactsTest
    {
        [TestMethod]
        public void GetContacts()
        {
            var controller = new ContactsController(new SampleContactRepository());
            var contacts = controller.Get();
            Assert.IsTrue(contacts.Any());
        }

        [TestMethod]
        public void Post()
        {
            // Post should return a contact
            var config = new HttpConfiguration();
            var kernel = new StandardKernel();
            kernel.Bind<IContactRepository>().ToConstant(new SampleContactRepository());
            WebApiConfig.Register(config, kernel);
            var server = new HttpServer(config);
            var client = new HttpClient(server);
            var contact = new Contact() { Name = "Test" };
            var response = client.PostAsJsonAsync<Contact>("http://localhost/api/contacts", contact).Result;
            var postedContact = response.Content.ReadAsAsync<Contact>().Result;
            Assert.IsNotNull(postedContact);

            // Post response should include a valid location header
            response = client.GetAsync(response.Headers.Location).Result;
            contact = response.Content.ReadAsAsync<Contact>().Result;
            Assert.IsNotNull(contact);

        }

        [TestMethod]
        public void GetContact()
        {
            var controller = new ContactsController(new SampleContactRepository());
            int id = 1;
            var contact = controller.Get(id);
            Assert.AreEqual(id, contact.ContactId);
        }

        [TestMethod]
        public void Delete()
        {
            var repository = new SampleContactRepository();
            var controller = new ContactsController(repository);
            int id = 1;
            controller.Delete(id);
            Assert.IsNull(repository.Get(id));
        }

        [TestMethod]
        public void GetContactsWithHttpClient()
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                // NOTE: Don't use .Result in real applications!!!
                var response = client.GetAsync("http://idontknowyou.org/api/contacts").Result;
                var contacts = response.Content.ReadAsAsync<IEnumerable<Contact>>().Result;
                Assert.IsTrue(contacts.Any());
            }
        }
    }
}
