using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Telerik.JustMock.EntityFrameworkCore.Tests
{
    [TestClass]
    public class DbContextTests
    {
        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DbContextOne : DbContext
        {
            public DbSet<Person> People { get; set; }
        }

        [TestMethod]
        public void Prepare_ShouldAssignDbSetProperties()
        {
            var dbContext = Mock.Create<DbContextOne>().PrepareMock();
            Assert.IsNotNull(dbContext.People);
        }

        public class DbContextTwo : DbContext
        {
            public DbSet<Person> People { get; set; }
        }

        [TestMethod]
        public void Prepare_ShouldAssignIDbSetProperties()
        {
            var dbContext = Mock.Create<DbContextTwo>().PrepareMock();
            Assert.IsNotNull(dbContext.People);
        }

        [TestMethod]
        public void SetGeneric_ReturnsSet()
        {
            var dbContext = Mock.Create<DbContextOne>().PrepareMock();
            var people = dbContext.Set<Person>();
            Assert.AreSame(dbContext.People, people);
        }

        [TestMethod]
        public void Dispose_DoesNothing()
        {
            var dbContext = Mock.Create<DbContextOne>().PrepareMock();
            dbContext.Dispose();
            // success
        }

        public interface IMyDbContext
        {
            DbSet<Person> People { get; }
        }

        [TestMethod]
        public void Prepare_Interface_DbSetsInitialized()
        {
            var dbContext = EntityFrameworkMockCore.Create<IMyDbContext>();
            Assert.IsNotNull(dbContext.People);
        }
    }
}
