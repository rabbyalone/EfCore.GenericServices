﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using GenericServices;
using GenericServices.PublicButHidden;
using GenericServices.Setup;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;
using Tests.EfClasses;
using Tests.EfCode;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Tests.UnitTests.GenericServicesPublic
{
    public class TestDeleteWithQueryFilter
    {
        [Fact]
        public void TestDeleteWithQueryFilterOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                context.Database.EnsureCreated();
                var author = new SoftDelEntity {SoftDeleted = true};
                context.Add(author);
                context.SaveChanges();
            }
            using (var context = new TestDbContext(options))
            {
                var utData = context.SetupEntitiesDirect();
                var service = new CrudServices(context, utData.ConfigAndMapper);

                context.SoftDelEntities.Count().ShouldEqual(0);
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(1);

                //ATTEMPT
                service.DeleteAndSave<SoftDelEntity>(1);

                //VERIFY
                service.IsValid.ShouldBeFalse();
                service.GetAllErrors().ShouldEqual("Sorry, I could not find the Soft Del Entity you wanted to delete.");
            }
            using (var context = new TestDbContext(options))
            {
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(1);
            }    
        }
        
        [Fact]
        public void TestDeleteWithActionWithQueryFilterOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                context.Database.EnsureCreated();
                var author = new SoftDelEntity { SoftDeleted = true };
                context.Add(author);
                context.SaveChanges();
            }
            using (var context = new TestDbContext(options))
            {
                var utData = context.SetupEntitiesDirect();
                var service = new CrudServices(context, utData.ConfigAndMapper);

                context.SoftDelEntities.Count().ShouldEqual(0);
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(1);

                //ATTEMPT
                service.DeleteWithActionAndSave<SoftDelEntity>((dbContext, entity) =>
                {
                    var status = new StatusGenericHandler();
                    if (!entity.SoftDeleted)
                        status.AddError("Can't delete if not already soft deleted.");
                    return status;
                },1);

                //VERIFY
                service.IsValid.ShouldBeTrue(service.GetAllErrors());
                service.Message.ShouldEqual("Successfully deleted a Soft Del Entity");
            }
            using (var context = new TestDbContext(options))
            {
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(0);
            }
        }

        [Fact]
        public void TestDeleteWithActionWithQueryFilterError()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                context.Database.EnsureCreated();
                var author = new SoftDelEntity { SoftDeleted = false };
                context.Add(author);
                context.SaveChanges();
            }
            using (var context = new TestDbContext(options))
            {
                var utData = context.SetupEntitiesDirect();
                var service = new CrudServices(context, utData.ConfigAndMapper);

                context.SoftDelEntities.Count().ShouldEqual(1);
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(1);

                //ATTEMPT
                service.DeleteWithActionAndSave<SoftDelEntity>((dbContext, entity) =>
                {
                    var status = new StatusGenericHandler();
                    if (!entity.SoftDeleted)
                        status.AddError("Can't delete if not already soft deleted.");
                    return status;
                }, 1);

                //VERIFY
                service.IsValid.ShouldBeFalse();
                service.GetAllErrors().ShouldEqual("Can't delete if not already soft deleted.");
            }
            using (var context = new TestDbContext(options))
            {
                context.SoftDelEntities.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

    }
}