using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace libx
{
    public class MessageBox : IEnumerator
    {
        private static readonly List<MessageBox> _showed = new List<MessageBox>();
        private static readonly List<MessageBox> _hidden = new List<MessageBox>();
        private Text _content;
        private Text _textNo;
        private Text _textOk;

        private Text _title;
        private bool _visible = true;

        private MessageBox(string title, string content, Action<bool> completed, string ok, string no)
        {
            var request = Assets.LoadAsset(ResFormat.GetPrefab("MessageBox"), typeof(GameObject));
            gameObject = Object.Instantiate(request.asset) as GameObject;
            Assert.IsNotNull(gameObject, "gameObject != null");
            gameObject.name = title;
            request.Require(gameObject);

            Object.DontDestroyOnLoad(gameObject);

            _title = GetComponent<Text>("Title");
            _content = GetComponent<Text>("Content/Text");
            _textOk = GetComponent<Text>("Buttons/Ok/Text");
            _textNo = GetComponent<Text>("Buttons/No/Text");

            var ok1 = GetComponent<Button>("Buttons/Ok");
            var no1 = GetComponent<Button>("Buttons/No");
            ok1.onClick.AddListener(OnClickOk);
            no1.onClick.AddListener(OnClickNo);

            this.completed += completed;

            Init(title, content, ok, no);
        }

        private GameObject gameObject { get; set; }

        public Action<bool> completed { get; set; }

        public static void Dispose()
        {
            foreach (var item in _hidden) item.Destroy();

            _hidden.Clear();

            foreach (var item in _showed) item.Destroy();

            _showed.Clear();
        }

        public static void CloseAll()
        {
            for (var index = 0; index < _showed.Count; index++)
            {
                var messageBox = _showed[index];
                messageBox.Hide();
                _hidden.Add(messageBox);
            }

            _showed.Clear();
        }

        public static MessageBox Show(string title, string content, Action<bool> completed, string ok = "确定",
            string no = "取消")
        {
            if (_hidden.Count > 0)
            {
                var mb = _hidden[0];
                mb.completed = completed;
                mb.Init(title, content, ok, no);
                mb.gameObject.SetActive(true);
                _hidden.RemoveAt(0);
                return mb;
            }

            return new MessageBox(title, content, completed, ok, no);
        }

        private void Destroy()
        {
            _title = null;
            _textOk = null;
            _textNo = null;
            _content = null;
            Object.DestroyImmediate(gameObject);
            gameObject = null;
        }

        private void Init(string title, string content, string ok, string no)
        {
            _title.text = title;
            _content.text = content;
            _textOk.text = ok;
            _textNo.text = no;
            _showed.Add(this);
            _visible = true;
        }

        private T GetComponent<T>(string path) where T : Component
        {
            var trans = gameObject.transform.Find(path);
            return trans.GetComponent<T>();
        }

        private void OnClickNo()
        {
            HandleEvent(false);
        }

        private void OnClickOk()
        {
            HandleEvent(true);
        }

        private void HandleEvent(bool isOk)
        {
            Close();
            if (completed == null) return;
            completed(isOk);
            completed = null;
        }

        public void Close()
        {
            Hide();
            _hidden.Add(this);
            _showed.Remove(this);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            _visible = false;
        }

        #region IEnumerator implementation

        public bool MoveNext()
        {
            return _visible;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get { return null; }
        }

        #endregion
    }
}