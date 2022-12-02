using System.Collections.Generic;
using UnityEngine;

namespace libx
{
    public class Reference
    {
        public string name { get; set; }

        private List<Object> _requires = null;

        public bool IsUnused()
        {
            return refCount <= 0;
        }

        public void UpdateRequires()
        {
            if (_requires == null)
            {
                return;
            }
            for (var i = 0; i < _requires.Count; i++)
            {
                var item = _requires[i];
                if (item != null)
                    break;
                _requires.RemoveAt(i);
                i--;
            }
            if (_requires.Count == 0)
            {
                Release();
                _requires = null;
            }
        }

        public int refCount { get; private set; }

        public virtual void Retain()
        {
            refCount++;
        }

        public virtual void Release()
        {
            refCount--;
            if (refCount < 0)
            {
                Debug.LogErrorFormat("Release: {0} refCount < 0", name);
            } 
        }

        public void Require(Object obj)
        {
            if (refCount > 0)
            {
                Release();
            }
            if (_requires == null)
            {
                _requires = new List<Object>();
                Retain();
            }
            _requires.Add(obj);
        }

        public void Dequire(Object obj)
        {
            if (_requires == null)
            {
                return;
            }
            _requires.Remove(obj);
        }
    }
}
