using System.Linq;
using FluentNUnity.Shared;
using NUnit.Framework;

namespace Hybel.ObjectPooling.Tests
{
    public partial class object_pool_tests
    {
        public class the_try_get_method
        {
            [Test]
            public void try_get_returns_true_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.TryGet(out var obj).Should().BeTrue();
                obj.Should().NotBeNull();
            }

            [Test]
            public void try_get_adds_an_instance_to_pool_when_pool_is_empty()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;

                pool.TryGet(out _).Should().BeTrue();
                pool.CountAll.ShouldBeEquivalentTo(1);
            }

            [Test]
            public void try_get_activates_an_object_when_pool_is_full_and_some_are_inactive()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.TryGet(out _).Should().BeTrue();
                pool.CountActive.Should().Be(1);
                pool.CountInactive.Should().Be(MAX_POOL_SIZE - 1);
            }

            [Test]
            public void try_get_returns_null_when_pool_is_full_and_all_objects_are_active()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(MAX_POOL_SIZE).ToList(); // Get all the objects and to list so it enumerates.
                pool.TryGet(out _).Should().BeFalse();
            }

            [Test]
            public void try_get_calls_on_take_method()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;

                pool.TryGet(out var obj).Should().BeTrue();
                obj.Enabled.Should().BeTrue();
            }
        }
    }
}
