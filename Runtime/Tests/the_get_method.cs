using System.Linq;
using FluentNUnity.Shared;
using NUnit.Framework;

namespace Hybel.ObjectPooling.Tests
{
    public partial class object_pool_tests
    {
        public class the_get_method
        {
            // Get with no parameters
            [Test]
            public void get_doesnt_return_null_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get().Should().NotBeNull();
            }

            [Test]
            public void get_adds_an_instance_to_pool_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get();
                pool.CountAll.ShouldBeEquivalentTo(1);
            }

            [Test]
            public void get_activates_an_object_when_pool_is_full_and_some_are_inactive()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get();
                pool.CountActive.Should().Be(1);
                pool.CountInactive.Should().Be(MAX_POOL_SIZE - 1);
            }

            [Test]
            public void get_returns_null_when_pool_is_full_and_all_objects_are_active()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(MAX_POOL_SIZE).ToList(); // Get all the objects and to list so it enumerates.
                pool.Get().Should().BeNull();
            }

            [Test]
            public void get_calls_on_take_method()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                var obj = pool.Get();
                obj.Enabled.Should().BeTrue();
            }

            // Get with amount
            [Test]
            public void get_amount_doesnt_return_null_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(3).ToList().ForEach(obj => obj.Should().NotBeNull());
            }

            [Test]
            public void get_amount_returns_the_correct_number_of_objects_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive; // Max Size of 5
                pool.Get(3).Count().Should().Be(3);
                pool.Get(3).Count().Should().Be(2);
            }

            [Test]
            public void get_amount_adds_instances_to_pool_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(3).ToList();
                pool.CountAll.ShouldBeEquivalentTo(3);
            }

            [Test]
            public void get_amount_activates_objects_when_pool_is_full_and_some_are_inactive()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(3).ToList();
                pool.CountActive.Should().Be(3);
                pool.CountInactive.Should().Be(MAX_POOL_SIZE - 3);
            }

            [Test]
            public void get_amount_returns_an_empty_enumerable_when_pool_is_full_and_all_objects_are_active()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(MAX_POOL_SIZE).ToList(); // Get all the objects and to list so it enumerates.
                pool.Get(3).Count().Should().Be(0);
            }

            [Test]
            public void get_amount_calls_on_take_method()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(3).ToList().ForEach(obj => obj.Enabled.Should().BeTrue());
            }
        }
    }
}
