using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PooledScrollList
{
    public class Pool<T> : IDisposable where T : Component
    {
        private T _itemPrefab;
        private Transform _parentObject;
        private Transform _poolRoot;
        private Queue<T> _itemsQueue;

        public int Count
        {
            get { return _itemsQueue.Count; }
        }

        public Pool(T itemPrefab, Transform parentObject, int poolCapacity)
        {
            _itemPrefab = itemPrefab;
            _parentObject = parentObject;
            _itemsQueue = new Queue<T>(poolCapacity);

            _poolRoot = new GameObject(string.Format("[{0}] Pool", itemPrefab.GetType().Name)).transform;
            _poolRoot.SetParent(parentObject, false);

            for (var i = 0; i < poolCapacity; i++)
            {
                var item = Object.Instantiate(itemPrefab, _poolRoot);
                item.gameObject.SetActive(false);

                _itemsQueue.Enqueue(item);
            }
        }

        public T GetNext()
        {
            T item;

            if (_itemsQueue.Count > 0)
            {
                item = _itemsQueue.Dequeue();
                item.transform.SetParent(_parentObject, false);
                item.gameObject.SetActive(true);
            }
            else
            {
                item = Object.Instantiate(_itemPrefab, _parentObject);
            }

            return item;
        }

        public void Return(T item)
        {
            item.gameObject.SetActive(false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localEulerAngles = Vector3.zero;
            item.transform.SetParent(_poolRoot, false);

            _itemsQueue.Enqueue(item);
        }

        public void Dispose()
        {
            foreach (var item in _itemsQueue)
            {
                Object.Destroy(item.gameObject);
            }

            Object.Destroy(_poolRoot.gameObject);

            _itemPrefab = null;
            _parentObject = null;
            _poolRoot = null;
            _itemsQueue = null;
        }
    }
}