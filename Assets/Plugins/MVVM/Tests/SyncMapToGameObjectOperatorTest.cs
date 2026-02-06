using System.Collections;
using MVVM;
using NUnit.Framework;
using ObservableCollections;
using Plugins.MVVM.Runtime.Operators;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace Plugins.MVVM.Runtime.Tests
{
    public class SyncToGameObjectOperatorTest
    {
        private class TestViewModel
        {
            public int Value { get; }
            
            public TestViewModel(int value)
            {
                Value = value;
            }
        }
        
        private class TestView : MonoBehaviour
        {
            public int Value { get; set; }
        }
        
        private GameObject _container;
        private TestView _prefab;
        
        [SetUp]
        public void SetUp()
        {
            _container = new GameObject("Container");
            
            var prefabGo = new GameObject("Prefab");
            _prefab = prefabGo.AddComponent<TestView>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_container != null)
                Object.DestroyImmediate(_container);
            
            if (_prefab != null)
                Object.DestroyImmediate(_prefab.gameObject);
        }
        
        [UnityTest]
        public IEnumerator Constructor_AutoSyncsExistingValues()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            // Act
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null; // Wait one frame for instantiation
            
            // Assert
            Assert.AreEqual(3, _container.transform.childCount);
            Assert.IsNotNull(_container.transform.GetChild(0).GetComponent<TestView>());
            Assert.IsNotNull(_container.transform.GetChild(1).GetComponent<TestView>());
            Assert.IsNotNull(_container.transform.GetChild(2).GetComponent<TestView>());
        }
        
        [UnityTest]
        public IEnumerator OnAdd_FiresForExistingItems()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            var addedCount = 0;
            
            // Act
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) =>
                {
                    addedCount++;
                    view.Value = vm.Value;
                });
            
            yield return null;
            
            // Assert
            Assert.AreEqual(3, addedCount);
            Assert.AreEqual(1, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(2, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(3, _container.transform.GetChild(2).GetComponent<TestView>().Value);
        }
        
        [UnityTest]
        public IEnumerator OnRemove_DoesNotFireForExistingItems()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            var removedCount = 0;
            
            // Act
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnRemove((vm, view) => removedCount++);
            
            yield return null;
            
            // Assert
            Assert.AreEqual(0, removedCount);
        }
        
        [UnityTest]
        public IEnumerator Add_CreatesGameObject()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>();
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            // Act
            collection.Add(new TestViewModel(42));
            yield return null;
            
            // Assert
            Assert.AreEqual(1, _container.transform.childCount);
            Assert.IsNotNull(_container.transform.GetChild(0).GetComponent<TestView>());
        }
        
        [UnityTest]
        public IEnumerator Add_InsertsAtCorrectSiblingIndex()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var vm3 = new TestViewModel(3);
            var collection = new ObservableList<TestViewModel> { vm1, vm3 };
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => view.Value = vm.Value);
            
            yield return null;
            
            // Act
            collection.Insert(1, vm2);
            yield return null;
            
            // Assert
            Assert.AreEqual(3, _container.transform.childCount);
            Assert.AreEqual(1, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(2, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(3, _container.transform.GetChild(2).GetComponent<TestView>().Value);
        }
        
        [UnityTest]
        public IEnumerator Add_FiresOnAddCallback()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>();
            var addedVm = (TestViewModel)null;
            var addedView = (TestView)null;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) =>
                {
                    addedVm = vm;
                    addedView = view;
                });
            
            var vm = new TestViewModel(42);
            
            // Act
            collection.Add(vm);
            yield return null;
            
            // Assert
            Assert.AreSame(vm, addedVm);
            Assert.IsNotNull(addedView);
        }
        
        [UnityTest]
        public IEnumerator Remove_DestroysGameObject()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var vm3 = new TestViewModel(3);
            var collection = new ObservableList<TestViewModel> { vm1, vm2, vm3 };
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null;
            
            // Act
            collection.Remove(vm2);
            yield return null; // Wait for destruction
            
            // Assert
            Assert.AreEqual(2, _container.transform.childCount);
        }
        
        [UnityTest]
        public IEnumerator Remove_FiresOnRemoveCallback()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var collection = new ObservableList<TestViewModel> { vm1, vm2 };
            
            var removedVm = (TestViewModel)null;
            var removedView = (TestView)null;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnRemove((vm, view) =>
                {
                    removedVm = vm;
                    removedView = view;
                });
            
            yield return null;
            
            // Act
            collection.Remove(vm2);
            yield return null;
            
            // Assert
            Assert.AreSame(vm2, removedVm);
            Assert.IsNotNull(removedView); // View still exists when callback fires
        }
        
        [UnityTest]
        public IEnumerator Clear_DestroysAllGameObjects()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null;
            
            // Act
            collection.Clear();
            yield return null;
            
            // Assert
            Assert.AreEqual(0, _container.transform.childCount);
        }
        
        [UnityTest]
        public IEnumerator Clear_FiresOnRemoveCallbackForEachItem()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var removeCount = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnRemove((vm, view) => removeCount++);
            
            yield return null;
            
            // Act
            collection.Clear();
            yield return null;
            
            // Assert
            Assert.AreEqual(3, removeCount);
        }
        
        [UnityTest]
        public IEnumerator Clear_FiresOnClearCallback()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var clearCalled = false;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnClear(() => clearCalled = true);
            
            yield return null;
            
            // Act
            collection.Clear();
            yield return null;
            
            // Assert
            Assert.IsTrue(clearCalled);
        }
        
        [UnityTest]
        public IEnumerator OnChanged_FiresWhenItemIsModified()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var collection = new ObservableList<TestViewModel> { vm1, vm2 };
            
            var changedCount = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnChanged(() => changedCount++);
            
            yield return null;
            
            // Act
            collection[0] = vm1; // Trigger change event with same reference
            yield return null;
            
            collection.Sort((a, b) => a.Value.CompareTo(b.Value));
            yield return null;
            
            // Assert
            Assert.AreEqual(3, changedCount, "OnChanged should fire when item is modified");
        }
        
        [UnityTest]
        public IEnumerator OnChanged_DoesNotFireDuringInitialSync()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var changedCount = 0;
            
            // Act
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnChanged(() => changedCount++);
            
            yield return null;
            
            // Assert
            Assert.AreEqual(0, changedCount, "OnChanged should not fire during initial sync");
        }
        
        [UnityTest]
        public IEnumerator OnChanged_CanHaveMultipleSubscribers()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var collection = new ObservableList<TestViewModel> { vm1 };
            
            var callback1Count = 0;
            var callback2Count = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnChanged(() => callback1Count++)
                .OnChanged(() => callback2Count++);
            
            yield return null;
            
            // Act
            collection[0] = vm1;
            yield return null;
            
            // Assert
            Assert.AreEqual(1, callback1Count, "First callback should fire");
            Assert.AreEqual(1, callback2Count, "Second callback should fire");
        }
        
        [UnityTest]
        public IEnumerator Sort_ReordersSiblingIndices()
        {
            // Arrange
            var vm3 = new TestViewModel(3);
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var collection = new ObservableList<TestViewModel> { vm3, vm1, vm2 };
    
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => view.Value = vm.Value);
    
            yield return null;
    
            // Store instance IDs for reliable comparison
            var idBefore3 = _container.transform.GetChild(0).GetComponent<TestView>().GetInstanceID();
            var idBefore1 = _container.transform.GetChild(1).GetComponent<TestView>().GetInstanceID();
            var idBefore2 = _container.transform.GetChild(2).GetComponent<TestView>().GetInstanceID();
    
            // Act
            collection.Sort((a, b) => a.Value.CompareTo(b.Value));
            yield return null;
    
            // Assert
            Assert.AreEqual(3, _container.transform.childCount);
    
            // Same GameObjects, just reordered
            Assert.AreEqual(idBefore1, _container.transform.GetChild(0).GetComponent<TestView>().GetInstanceID());
            Assert.AreEqual(idBefore2, _container.transform.GetChild(1).GetComponent<TestView>().GetInstanceID());
            Assert.AreEqual(idBefore3, _container.transform.GetChild(2).GetComponent<TestView>().GetInstanceID());
    
            Assert.AreEqual(1, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(2, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(3, _container.transform.GetChild(2).GetComponent<TestView>().Value);
        }
        
        [UnityTest]
        public IEnumerator Sort_DoesNotDestroyGameObjects()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(3),
                new TestViewModel(1),
                new TestViewModel(2)
            };
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null;
            
            var childCountBefore = _container.transform.childCount;
            
            // Act
            collection.Sort((a, b) => a.Value.CompareTo(b.Value));
            yield return null;
            
            // Assert
            Assert.AreEqual(childCountBefore, _container.transform.childCount);
        }
        
        [UnityTest]
        public IEnumerator Sort_DoesNotFireCallbacks()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(3),
                new TestViewModel(1),
                new TestViewModel(2)
            };
            
            var addCount = 0;
            var removeCount = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => addCount++)
                .OnRemove((vm, view) => removeCount++);
            
            yield return null;
            
            addCount = 0; // Reset after initial sync
            
            // Act
            collection.Sort((a, b) => a.Value.CompareTo(b.Value));
            yield return null;
            
            // Assert
            Assert.AreEqual(0, addCount);
            Assert.AreEqual(0, removeCount);
        }
        
        [UnityTest]
        public IEnumerator Dispose_DestroysAllGameObjects()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null;
            
            // Act
            op.Dispose();
            yield return null;
            
            // Assert
            Assert.AreEqual(0, _container.transform.childCount);
        }
        
        [UnityTest]
        public IEnumerator Dispose_StopsListeningToCollection()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                collection, _prefab, _container.transform);
            
            yield return null;
            
            // Act
            op.Dispose();
            yield return null;
            
            collection.Add(new TestViewModel(4));
            yield return null;
            
            // Assert
            Assert.AreEqual(0, _container.transform.childCount); // Should not have added the 4th item
        }
        
        [UnityTest]
        public IEnumerator Move_PreservesGameObjectsAndReorders()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var vm3 = new TestViewModel(3);
            var collection = new ObservableList<TestViewModel> { vm1, vm2, vm3 };
            
            var addCount = 0;
            var removeCount = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => { view.Value = vm.Value; addCount++; })
                .OnRemove((vm, view) => removeCount++);
            
            yield return null;
            
            var idBefore1 = _container.transform.GetChild(0).GetComponent<TestView>().GetInstanceID();
            addCount = 0; // Reset after initial sync
            
            // Act - Move vm1 from index 0 to index 2
            collection.Move(0, 2);
            yield return null;
            
            // Assert - Same GameObjects, reordered, no add/remove
            Assert.AreEqual(3, _container.transform.childCount, "Should still have 3 views");
            Assert.AreEqual(2, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(3, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(1, _container.transform.GetChild(2).GetComponent<TestView>().Value);
            Assert.AreEqual(idBefore1, _container.transform.GetChild(2).GetComponent<TestView>().GetInstanceID(), 
                "Should reuse same GameObject");
            Assert.AreEqual(0, addCount, "Should not add during move");
            Assert.AreEqual(0, removeCount, "Should not remove during move");
        }

        [UnityTest]
        public IEnumerator Reverse_PreservesGameObjectsAndReorders()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
            
            var addCount = 0;
            var removeCount = 0;
            
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => { view.Value = vm.Value; addCount++; })
                .OnRemove((vm, view) => removeCount++);
            
            yield return null;
            
            var idBefore1 = _container.transform.GetChild(0).GetComponent<TestView>().GetInstanceID();
            var idBefore3 = _container.transform.GetChild(2).GetComponent<TestView>().GetInstanceID();
            addCount = 0;
            
            // Act
            collection.Reverse();
            yield return null;
            
            // Assert - Same GameObjects, reversed order, no add/remove
            Assert.AreEqual(3, _container.transform.childCount, "Should still have 3 views");
            Assert.AreEqual(3, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(2, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(1, _container.transform.GetChild(2).GetComponent<TestView>().Value);
            Assert.AreEqual(idBefore3, _container.transform.GetChild(0).GetComponent<TestView>().GetInstanceID(),
                "Should reuse same GameObjects");
            Assert.AreEqual(idBefore1, _container.transform.GetChild(2).GetComponent<TestView>().GetInstanceID(),
                "Should reuse same GameObjects");
            Assert.AreEqual(0, addCount, "Should not add during reverse");
            Assert.AreEqual(0, removeCount, "Should not remove during reverse");
        }
        
        [UnityTest]
        public IEnumerator Replace_RemovesOldAndAddsNew()
        {
            // Arrange
            var vm1 = new TestViewModel(1);
            var vm2 = new TestViewModel(2);
            var vmNew = new TestViewModel(99);
            var collection = new ObservableList<TestViewModel> { vm1, vm2 };
    
            var removeCount = 0;
            var addCount = 0;
    
            using var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnAdd((vm, view) => { view.Value = vm.Value; addCount++; })
                .OnRemove((vm, view) => removeCount++);
    
            yield return null;
    
            addCount = 0; // Reset after initial sync
    
            // Act
            collection[1] = vmNew; // Replace vm2 with vmNew
            yield return null;
    
            // Assert
            Assert.AreEqual(2, _container.transform.childCount);
            Assert.AreEqual(1, _container.transform.GetChild(0).GetComponent<TestView>().Value);
            Assert.AreEqual(99, _container.transform.GetChild(1).GetComponent<TestView>().Value);
            Assert.AreEqual(1, removeCount, "Should remove old ViewModel");
            Assert.AreEqual(1, addCount, "Should add new ViewModel");
        }
        
        [UnityTest]
        public IEnumerator Dispose_FiresOnRemoveCallbacks()
        {
            // Arrange
            var collection = new ObservableList<TestViewModel>
            {
                new TestViewModel(1),
                new TestViewModel(2),
                new TestViewModel(3)
            };
    
            var removeCount = 0;
    
            var op = new SyncToGameObjectOperator<TestViewModel, TestView>(
                    collection, _prefab, _container.transform)
                .OnRemove((vm, view) => removeCount++);
    
            yield return null;
    
            // Act
            op.Dispose();
            yield return null;
    
            // Assert
            Assert.AreEqual(3, removeCount, "Should fire OnRemove for all items on dispose");
        }
        
        
    }
}