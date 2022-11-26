using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
// using UnityEngine.Rendering;


[assembly: Preserve]
enum IntEnum : int
{
    A,
    B,
}


namespace BPHuatuo
{   
    
    // 做一个观察者示例出来,解决 UniRx.Observable::Repeat<System.Int64>(System.IObservable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
    public class BPObservable<T> : System.IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return default;
        }
    }

    /// <summary>
    /// 这个类,是我们项目缺少的.根据真机调测出来,一个一个改的
    /// </summary>
    public class HuatuoRefTypes : MonoBehaviour
    {
        public void MyAOTRefs()
        {
            var builder = new System.Runtime.CompilerServices.AsyncVoidMethodBuilder();
            var v = default(IAsyncStateMachine);
            builder.Start(ref v);
            
            System.Linq.Enumerable.Skip<bool>((IEnumerable<bool>)null, 0);
            System.Linq.Enumerable.Skip<object>((IEnumerable<object>)null, 0);
            System.Linq.Enumerable.ElementAt<object>((IEnumerable<object>)null, 0);
            System.Linq.Enumerable.ElementAt<KeyValuePair<int, object>>((IEnumerable<KeyValuePair<int, object>>)null, 0);
            
            // MissingMethodException: MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]],System.String>(System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5
            System.Linq.Enumerable.Select<KeyValuePair<string, int>, string>((IEnumerable<KeyValuePair<string, int>>)null, (System.Func<KeyValuePair<string, int>, string>)null);
            
            // MissingMethodException: MethodPointerNotImplement System.Linq.IOrderedEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::OrderBy<System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, Public
            System.Linq.Enumerable.OrderBy<KeyValuePair<string, int>, int>((IEnumerable<KeyValuePair<string, int>>)null, (System.Func<KeyValuePair<string, int>, int>)null);

            // 2022-05-23 16:46:41.457 18816-18858 E/Unity: MissingMethodException: MethodPointerNotImplement System.String System.String::Join<System.Int32>(System.String,System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
            System.String.Join<int>(default, (IEnumerable<int>)null);

            // MissingMethodException: MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::Where<System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neu
            System.Linq.Enumerable.Skip<KeyValuePair<int, object>>((IEnumerable<KeyValuePair<int, object>>)null, 0);
            // 上面那个测试没用,但是先留着.这个应该才是对的(下面这个测试通过)
            System.Linq.Enumerable.Where<KeyValuePair<int, object>>((IEnumerable<KeyValuePair<int, object>>)null, (System.Func<KeyValuePair<int, object>, bool>)null);

            // MissingMethodException: MethodPointerNotImplement System.Linq.IOrderedEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::OrderBy<System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neutral,
            System.Linq.Enumerable.OrderBy<KeyValuePair<int, object>, int>((IEnumerable<KeyValuePair<int, object>>)null, (System.Func<KeyValuePair<int, object>, int>)null);

            // MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[BPGames.BPEnemyWaveInfo, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPGames.BPEnemyWaveInfo>(System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b7
            System.Linq.Enumerable.Select<KeyValuePair<int, object>, object>((IEnumerable<KeyValuePair<int, object>>)null, (System.Func<KeyValuePair<int, object>, object>)null);

            // MissingMethodException: MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[UnityEngine.SpriteRenderer, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[BPGames.BPUtility+AvatarPlaceType, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::Where<System.Collections.Generic.KeyValuePair`2[[UnityEngine.SpriteRenderer, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[BPGames.BPUtility+Av
            System.Linq.Enumerable.Where<KeyValuePair<object, IntEnum>>((IEnumerable<KeyValuePair<object, IntEnum>>)null, (System.Func<KeyValuePair<object, IntEnum>, bool>)null);
            System.Linq.Enumerable.Where<KeyValuePair<int, IntEnum>>((IEnumerable<KeyValuePair<int, IntEnum>>)null, (System.Func<KeyValuePair<int, IntEnum>, bool>)null);
            
            // MissingMethodException: MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[UnityEngine.SpriteRenderer, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2[[UnityEngine.SpriteRenderer, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[BPGames.BPUtility+AvatarPlaceType, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],UnityEngine.SpriteRenderer>(System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[UnityEngine.SpriteRenderer,
            System.Linq.Enumerable.Select<KeyValuePair<object, IntEnum>, object>((IEnumerable<KeyValuePair<object, IntEnum>>)null, (System.Func<KeyValuePair<object, IntEnum>, object>)null);

            // 直接new ScriptableObject. 先跳过去哪个代码先
            ScriptableObject so1 = new ScriptableObject();
            ScriptableObject.CreateInstance("");

            // MissingMethodException: MethodPointerNotImplement System.IObservable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] UniRx.Observable::Repeat<System.Int64>(System.IObservable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
            BPObservable<System.Int16> bpObservable1 = new BPObservable<System.Int16>();
            BPObservable<System.Int32> bpObservable2 = new BPObservable<System.Int32>();
            BPObservable<System.Int64> bpObservable3 = new BPObservable<System.Int64>();
            BPObservable<long> bpObservable4 = new BPObservable<long>();
            BPObservable<float> bpObservable5 = new BPObservable<float>();
            BPObservable<object> bpObservable6 = new BPObservable<object>();
            BPObservable<System.Int64> bpObservable7 = new BPObservable<System.Int64>();
            

            // MissingMethodException: MethodNotFind UnityEngine.AsyncOperation::set_allowSceneActivation
            // UnityEngine.AsyncOperation
            // 直接在link.xml 里
            // <assembly fullname="UnityEngine.CoreModule">
		    //  <type fullname="UnityEngine.AsyncOperation" preserve="all"/>
            
            // 另外一个示例,应该是加类的名
            //  <type fullname="UnityEngine.Rendering.SortingGroup" preserve="all"/>


            // MissingMethodException: MethodPointerNotImplement System.Collections.Generic.IEnumerable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::Select<System.Int32,System.String>(System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]],System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
            System.Linq.Enumerable.Select<int, string>((IEnumerable<int>)null, (System.Func<int, string>)null);
            System.Linq.Enumerable.Select<System.Int32, string>((IEnumerable<System.Int32>)null, (System.Func<System.Int32, string>)null);
            System.Linq.Enumerable.Select<System.Int64, string>((IEnumerable<System.Int64>)null, (System.Func<System.Int64, string>)null);

            // MissingMethodException: MethodNotFind UnityEngine.Tilemaps.TilemapRenderer::set_maskInteraction
            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[UnityEngine.Rect, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]::.ctor()
            Dictionary<int, UnityEngine.Rect> dic1 = new Dictionary<int, UnityEngine.Rect>();

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Boolean System.Linq.Enumerable::Contains<BPGames.ITEM_TYPE>(System.Collections.Generic.IEnumerable`1[[BPGames.ITEM_TYPE, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPGames.ITEM_TYPE) at UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) [0x00000] in <00000000000000000000000000000000>:0 
            System.Linq.Enumerable.Contains<IntEnum>((IEnumerable<IntEnum>)null, 0);
            System.Linq.Enumerable.Contains<System.Int16>((IEnumerable<System.Int16>)null, 0);
            System.Linq.Enumerable.Contains<System.Int32>((IEnumerable<System.Int32>)null, 0);
            System.Linq.Enumerable.Contains<System.Int64>((IEnumerable<System.Int64>)null, 0);
            
            System.Linq.Enumerable.Contains<System.UInt16>((IEnumerable<System.UInt16>)null, 0);
            System.Linq.Enumerable.Contains<System.UInt32>((IEnumerable<System.UInt32>)null, 0);
            System.Linq.Enumerable.Contains<System.UInt64>((IEnumerable<System.UInt64>)null, 0);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Collections.Generic.Stack`1[[System.Char, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::.ctor()
            System.Collections.Generic.Stack<char> stack1 = new System.Collections.Generic.Stack<char>();
            System.Collections.Generic.Stack<System.Int16> stack2 = new System.Collections.Generic.Stack<System.Int16>();
            System.Collections.Generic.Stack<short> stack3 = new System.Collections.Generic.Stack<short>();
            System.Collections.Generic.Stack<System.Int32> stack4 = new System.Collections.Generic.Stack<System.Int32>();
            System.Collections.Generic.Stack<System.Int64> stack5 = new System.Collections.Generic.Stack<System.Int64>();
            System.Collections.Generic.Stack<object> stack6 = new System.Collections.Generic.Stack<object>();
            System.Collections.Generic.Stack<System.Single> stack7 = new System.Collections.Generic.Stack<System.Single>();

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Collections.Generic.KeyValuePair`2[[BPGames.BPGameObject, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][] System.Linq.Enumerable::ToArray<System.Collections.Generic.KeyValuePair`2[[BPGames.BPGameObject, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]>(System.Collections.Generic.IEnumerable`1[[System.Collections.Ge
            System.Linq.Enumerable.ToArray<KeyValuePair<object, bool>>((IEnumerable<KeyValuePair<object, bool>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<object, IntEnum>>((IEnumerable<KeyValuePair<object, IntEnum>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<object, int>>((IEnumerable<KeyValuePair<object, int>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<object, object>>((IEnumerable<KeyValuePair<object, object>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<string, object>>((IEnumerable<KeyValuePair<string, object>>)null);
            System.Linq.Enumerable.ToArray<KeyValuePair<object, string>>((IEnumerable<KeyValuePair<object, string>>)null);

            // MissingMethodException: MethodNotFind UnityEngine.LineRenderer::set_positionCount

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Linq.Enumerable::Select<BPGames.BPActor,System.Int32>(System.Collections.Generic.IEnumerable`1[[BPGames.BPActor, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],System.Func`2[[BPGames.BPActor, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
            System.Linq.Enumerable.Select<object, System.Int32>((IEnumerable<object>)null, (System.Func<object, System.Int32>)null);
            System.Linq.Enumerable.Select<object, System.Int64>((IEnumerable<object>)null, (System.Func<object, System.Int64>)null);
            System.Linq.Enumerable.Select<object, IntEnum>((IEnumerable<object>)null, (System.Func<object, IntEnum>)null);
            System.Linq.Enumerable.Select<object, object>((IEnumerable<object>)null, (System.Func<object, object>)null);
            System.Linq.Enumerable.Select<string, object>((IEnumerable<string>)null, (System.Func<string, object>)null);
            System.Linq.Enumerable.Select<string, string>((IEnumerable<string>)null, (System.Func<string, string>)null);
            System.Linq.Enumerable.Select<object, string>((IEnumerable<object>)null, (System.Func<object, string>)null);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Collections.Generic.IEnumerable`1[[UnityEngine.Vector2, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] System.Linq.Enumerable::Select<BPGames.BPActor,UnityEngine.Vector2>(System.Collections.Generic.IEnumerable`1[[BPGames.BPActor, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],System.Func`2[[BPGames.BPActor, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[UnityEngine.Vector2, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]])
            System.Linq.Enumerable.Select<object, Vector2>((IEnumerable<object>)null, (System.Func<object, Vector2>)null);
            System.Linq.Enumerable.Select<object, Vector2Int>((IEnumerable<object>)null, (System.Func<object, Vector2Int>)null);
            System.Linq.Enumerable.Select<object, Vector2>((IEnumerable<object>)null, (System.Func<object, Vector2>)null);
            System.Linq.Enumerable.Select<object, Vector3Int>((IEnumerable<object>)null, (System.Func<object, Vector3Int>)null);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Linq.IOrderedEnumerable`1[[BPGames.BPInfo_MazeMissionGroup, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] System.Linq.Enumerable::OrderByDescending<BPGames.BPInfo_MazeMissionGroup,System.Int32>(System.Collections.Generic.IEnumerable`1[[BPGames.BPInfo_MazeMissionGroup, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],System.Func`2[[BPGames.BPInfo_MazeMissionGroup, BPGames, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=
            System.Linq.Enumerable.OrderByDescending<object, System.Int32>((IEnumerable<object>)null, (System.Func<object, System.Int32>)null);
            System.Linq.Enumerable.OrderByDescending<object, System.Int64>((IEnumerable<object>)null, (System.Func<object, System.Int64>)null);
            System.Linq.Enumerable.OrderByDescending<string, System.Int32>((IEnumerable<string>)null, (System.Func<string, System.Int32>)null);
            System.Linq.Enumerable.OrderByDescending<string, System.Int64>((IEnumerable<string>)null, (System.Func<string, System.Int64>)null);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Collections.Generic.Dictionary`2[[UnityEngine.Vector2, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]::.ctor()
            new Dictionary<UnityEngine.Vector2, UnityEngine.GameObject>();
            new Dictionary<UnityEngine.Vector2, object>();
            new Dictionary<UnityEngine.Vector2Int, UnityEngine.GameObject>();
            new Dictionary<UnityEngine.Vector2Int, object>();
            new Dictionary<UnityEngine.Vector3, UnityEngine.GameObject>();
            new Dictionary<UnityEngine.Vector3, object>();
            new Dictionary<UnityEngine.Vector3Int, UnityEngine.GameObject>();
            new Dictionary<UnityEngine.Vector3Int, object>();

            // MissingMethodException: MethodNotFind UnityEngine.Networking.UnityWebRequest::Get

        }



        List<Type> GetTypes()
        {
            // 调用一下? 防止剪裁?
            this.MyAOTRefs();

            return new List<Type>
            {
                    
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(GetTypes());
            GameObject.Instantiate<GameObject>(null);
            Instantiate<GameObject>(null, null);
            Instantiate<GameObject>(null, null, false);
            Instantiate<GameObject>(null, new Vector3(), new Quaternion());
            Instantiate<GameObject>(null, new Vector3(), new Quaternion(), null);
        }
        void RefNullable()
        {
            // nullable
            object b = null;
            int? a = 5;
            b = a;
            int d = (int?)b ?? 7;
            int e = (int)b;
            a = d;
            b = a;
            b = Enumerable.Range(0, 1).Reverse().Take(1).TakeWhile(x => true).Skip(1).All(x => true);
            b = new WaitForSeconds(1f);
            b = new WaitForSecondsRealtime(1f);
            b = new WaitForFixedUpdate();
            b = new WaitForEndOfFrame();
            b = new WaitWhile(() => true);
            b = new WaitUntil(() => true);
        }
        void RefContainer()
        {
            List<object> ronDic = new List<object>()
            {

            };

            //int, long,float,double, IntEnum,object
            List<object> b = new List<object>()
            {
                new Vector2Int(1, 1),
            new Dictionary<int, int>(),
            new Dictionary<int, bool>(),
            new Dictionary<int, float>(),
            new Dictionary<int, double>(),
            new Dictionary<int, object>(),
            new Dictionary<int, IntEnum>(),
            new Dictionary<int, Vector2Int>(),
            new Dictionary<Vector2Int, object>(),
            
            new Dictionary<ushort, int>(),
            new Dictionary<ushort, ushort>(),
            new Dictionary<ushort, float>(),
            new Dictionary<ushort, double>(),
            new Dictionary<ushort, object>(),
            new Dictionary<ushort, IntEnum>(),

            new Dictionary<string, string>(),
            new Dictionary<string, int>(),
            new Dictionary<string, ushort>(),
            new Dictionary<string, float>(),
            new Dictionary<string, double>(),
            new Dictionary<string, object>(),
            new Dictionary<string, IntEnum>(),
            new Dictionary<long, long>(),
            new Dictionary<long, int>(),
            new Dictionary<long, float>(),
            new Dictionary<long, double>(),
            new Dictionary<long, object>(),
            new Dictionary<long, IntEnum>(),
            new Dictionary<float, int>(),
            new Dictionary<float, long>(),
            new Dictionary<float, double>(),
            new Dictionary<float, object>(),
            new Dictionary<float, IntEnum>(),
            new Dictionary<double, int>(),
            new Dictionary<double, long>(),
            new Dictionary<double, float>(),
            new Dictionary<double, object>(),
            new Dictionary<double, IntEnum>(),
            new Dictionary<object, int>(),
            new Dictionary<object, long>(),
            new Dictionary<object, float>(),
            new Dictionary<object, double>(),
            new Dictionary<object, ushort>(),
            new Dictionary<object, IntEnum>(),
            new Dictionary<IntEnum, int>(),
            new Dictionary<IntEnum, long>(),
            new Dictionary<IntEnum, float>(),
            new Dictionary<IntEnum, double>(),
            new Dictionary<IntEnum, object>(),
            new Dictionary<(int, long), object>(),
            new Dictionary<(int, float), object>(),
            new Dictionary<(int, double), object>(),
            new Dictionary<(int, object), object>(),
            new Dictionary<(int, IntEnum), object>(),
            new Dictionary<(long, int), object>(),
            new Dictionary<(long, float), object>(),
            new Dictionary<(long, double), object>(),
            new Dictionary<(long, object), object>(),
            new Dictionary<(long, IntEnum), object>(),
            new Dictionary<(float, int), object>(),
            new Dictionary<(float, long), object>(),
            new Dictionary<(float, double), object>(),
            new Dictionary<(float, object), object>(),
            new Dictionary<(float, IntEnum), object>(),
            new Dictionary<(double, int), object>(),
            new Dictionary<(double, long), object>(),
            new Dictionary<(double, float), object>(),
            new Dictionary<(double, object), object>(),
            new Dictionary<(double, IntEnum), object>(),
            new Dictionary<(object, int), object>(),
            new Dictionary<(object, long), object>(),
            new Dictionary<(object, float), object>(),
            new Dictionary<(object, double), object>(),
            new Dictionary<(object, IntEnum), object>(),
            new Dictionary<(IntEnum, int), object>(),
            new Dictionary<(IntEnum, long), object>(),
            new Dictionary<(IntEnum, float), object>(),
            new Dictionary<(IntEnum, double), object>(),
            new Dictionary<(IntEnum, object), object>(),
            new Dictionary<(int, long, float), object>(),
            new Dictionary<(int, long, double), object>(),
            new Dictionary<(int, long, object), object>(),
            new Dictionary<(int, long, IntEnum), object>(),
            new Dictionary<(int, float, long), object>(),
            new Dictionary<(int, float, double), object>(),
            new Dictionary<(int, float, object), object>(),
            new Dictionary<(int, float, IntEnum), object>(),
            new Dictionary<(int, double, long), object>(),
            new Dictionary<(int, double, float), object>(),
            new Dictionary<(int, double, object), object>(),
            new Dictionary<(int, double, IntEnum), object>(),
            new Dictionary<(int, object, long), object>(),
            new Dictionary<(int, object, float), object>(),
            new Dictionary<(int, object, double), object>(),
            new Dictionary<(int, object, IntEnum), object>(),
            new Dictionary<(int, IntEnum, long), object>(),
            new Dictionary<(int, IntEnum, float), object>(),
            new Dictionary<(int, IntEnum, double), object>(),
            new Dictionary<(int, IntEnum, object), object>(),
            new Dictionary<(long, int, float), object>(),
            new Dictionary<(long, int, double), object>(),
            new Dictionary<(long, int, object), object>(),
            new Dictionary<(long, int, IntEnum), object>(),
            new Dictionary<(long, float, int), object>(),
            new Dictionary<(long, float, double), object>(),
            new Dictionary<(long, float, object), object>(),
            new Dictionary<(long, float, IntEnum), object>(),
            new Dictionary<(long, double, int), object>(),
            new Dictionary<(long, double, float), object>(),
            new Dictionary<(long, double, object), object>(),
            new Dictionary<(long, double, IntEnum), object>(),
            new Dictionary<(long, object, int), object>(),
            new Dictionary<(long, object, float), object>(),
            new Dictionary<(long, object, double), object>(),
            new Dictionary<(long, object, IntEnum), object>(),
            new Dictionary<(long, IntEnum, int), object>(),
            new Dictionary<(long, IntEnum, float), object>(),
            new Dictionary<(long, IntEnum, double), object>(),
            new Dictionary<(long, IntEnum, object), object>(),
            new Dictionary<(float, int, long), object>(),
            new Dictionary<(float, int, double), object>(),
            new Dictionary<(float, int, object), object>(),
            new Dictionary<(float, int, IntEnum), object>(),
            new Dictionary<(float, long, int), object>(),
            new Dictionary<(float, long, double), object>(),
            new Dictionary<(float, long, object), object>(),
            new Dictionary<(float, long, IntEnum), object>(),
            new Dictionary<(float, double, int), object>(),
            new Dictionary<(float, double, long), object>(),
            new Dictionary<(float, double, object), object>(),
            new Dictionary<(float, double, IntEnum), object>(),
            new Dictionary<(float, object, int), object>(),
            new Dictionary<(float, object, long), object>(),
            new Dictionary<(float, object, double), object>(),
            new Dictionary<(float, object, IntEnum), object>(),
            new Dictionary<(float, IntEnum, int), object>(),
            new Dictionary<(float, IntEnum, long), object>(),
            new Dictionary<(float, IntEnum, double), object>(),
            new Dictionary<(float, IntEnum, object), object>(),
            new Dictionary<(double, int, long), object>(),
            new Dictionary<(double, int, float), object>(),
            new Dictionary<(double, int, object), object>(),
            new Dictionary<(double, int, IntEnum), object>(),
            new Dictionary<(double, long, int), object>(),
            new Dictionary<(double, long, float), object>(),
            new Dictionary<(double, long, object), object>(),
            new Dictionary<(double, long, IntEnum), object>(),
            new Dictionary<(double, float, int), object>(),
            new Dictionary<(double, float, long), object>(),
            new Dictionary<(double, float, object), object>(),
            new Dictionary<(double, float, IntEnum), object>(),
            new Dictionary<(double, object, int), object>(),
            new Dictionary<(double, object, long), object>(),
            new Dictionary<(double, object, float), object>(),
            new Dictionary<(double, object, IntEnum), object>(),
            new Dictionary<(double, IntEnum, int), object>(),
            new Dictionary<(double, IntEnum, long), object>(),
            new Dictionary<(double, IntEnum, float), object>(),
            new Dictionary<(double, IntEnum, object), object>(),
            new Dictionary<(object, int, long), object>(),
            new Dictionary<(object, int, float), object>(),
            new Dictionary<(object, int, double), object>(),
            new Dictionary<(object, int, IntEnum), object>(),
            new Dictionary<(object, long, int), object>(),
            new Dictionary<(object, long, float), object>(),
            new Dictionary<(object, long, double), object>(),
            new Dictionary<(object, long, IntEnum), object>(),
            new Dictionary<(object, float, int), object>(),
            new Dictionary<(object, float, long), object>(),
            new Dictionary<(object, float, double), object>(),
            new Dictionary<(object, float, IntEnum), object>(),
            new Dictionary<(object, double, int), object>(),
            new Dictionary<(object, double, long), object>(),
            new Dictionary<(object, double, float), object>(),
            new Dictionary<(object, double, IntEnum), object>(),
            new Dictionary<(object, IntEnum, int), object>(),
            new Dictionary<(object, IntEnum, long), object>(),
            new Dictionary<(object, IntEnum, float), object>(),
            new Dictionary<(object, IntEnum, double), object>(),
            new Dictionary<(IntEnum, int, long), object>(),
            new Dictionary<(IntEnum, int, float), object>(),
            new Dictionary<(IntEnum, int, double), object>(),
            new Dictionary<(IntEnum, int, object), object>(),
            new Dictionary<(IntEnum, long, int), object>(),
            new Dictionary<(IntEnum, long, float), object>(),
            new Dictionary<(IntEnum, long, double), object>(),
            new Dictionary<(IntEnum, long, object), object>(),
            new Dictionary<(IntEnum, float, int), object>(),
            new Dictionary<(IntEnum, float, long), object>(),
            new Dictionary<(IntEnum, float, double), object>(),
            new Dictionary<(IntEnum, float, object), object>(),
            new Dictionary<(IntEnum, double, int), object>(),
            new Dictionary<(IntEnum, double, long), object>(),
            new Dictionary<(IntEnum, double, float), object>(),
            new Dictionary<(IntEnum, double, object), object>(),
            new Dictionary<(IntEnum, object, int), object>(),
            new Dictionary<(IntEnum, object, long), object>(),
            new Dictionary<(IntEnum, object, float), object>(),
            new Dictionary<(IntEnum, object, double), object>(),
            new SortedDictionary<int, int>(),
            new SortedDictionary<int, long>(),
            new SortedDictionary<int, float>(),
            new SortedDictionary<int, double>(),
            new SortedDictionary<int, object>(),
            new SortedDictionary<int, IntEnum>(),
            new SortedDictionary<long, int>(),
            new SortedDictionary<long, long>(),
            new SortedDictionary<long, float>(),
            new SortedDictionary<long, double>(),
            new SortedDictionary<long, object>(),
            new SortedDictionary<long, IntEnum>(),
            new SortedDictionary<float, int>(),
            new SortedDictionary<float, long>(),
            new SortedDictionary<float, double>(),
            new SortedDictionary<float, object>(),
            new SortedDictionary<float, IntEnum>(),
            new SortedDictionary<double, int>(),
            new SortedDictionary<double, long>(),
            new SortedDictionary<double, float>(),
            new SortedDictionary<double, object>(),
            new SortedDictionary<double, IntEnum>(),
            new SortedDictionary<object, object>(),
            new SortedDictionary<object, int>(),
            new SortedDictionary<object, long>(),
            new SortedDictionary<object, float>(),
            new SortedDictionary<object, double>(),
            new SortedDictionary<object, IntEnum>(),
            new SortedDictionary<IntEnum, int>(),
            new SortedDictionary<IntEnum, long>(),
            new SortedDictionary<IntEnum, float>(),
            new SortedDictionary<IntEnum, double>(),
            new SortedDictionary<IntEnum, object>(),
            new HashSet<ushort>(),
            new HashSet<int>(),
            new HashSet<long>(),
            new HashSet<float>(),
            new HashSet<double>(),
            new HashSet<object>(),
            new HashSet<IntEnum>(),
            new HashSet<string>(),
            new List<int>(),
            new List<long>(),
            new List<Single>(),
            new List<float>(),
            new List<double>(),
            new List<object>(),
            new List<string>(),
            new List<Vector3>(),
            new List<IntEnum>(),
            new List<(long, object)>(),
            new List<ValueTuple<object, long>>(),
            new List<ValueTuple<int, int>>(),
            new List<ValueTuple<int, int, int>>(),
            new List<ValueTuple<int, int, long>>(),
            new List<ValueTuple<int, long>>(),
            new List<ValueTuple<int, object>>(),
            new List<ValueTuple<long, int>>(),
            new List<ValueTuple<int, bool>>(),
            new List<ValueTuple<bool, bool>>(),

            new Queue<int>(),
            new Queue<long>(),
            new Queue<float>(),
            new Queue<double>(),
            new Queue<object>(),
            new Queue<string>(),
            new Queue<IntEnum>(),
            new Queue<(int, long, int)>(),
            new Queue<(int, int, int)>(),
            new Queue<(long, long, long)>(),
            new Queue<(float, float, float)>(),
            new Queue<(long, object)>(),
            new Queue<(object, long)>(),
            new Queue<ValueTuple<object, long>>(),
            new Queue<ValueTuple<int, int>>(),
            new Queue<ValueTuple<int, object>>(),
            new Queue<ValueTuple<int, long>>(),
            new Queue<ValueTuple<long, long>>(),
            new Queue<ValueTuple<object, int>>(),
            new Queue<ValueTuple<object, object>>(),

            // new Queue<(int, long, int)>(),

            new ValueTuple<int>(1),
            new ValueTuple<long>(1),
            new ValueTuple<float>(1f),
            new ValueTuple<double>(1),
            new ValueTuple<object>(null),
            new ValueTuple<IntEnum>(IntEnum.A),
            new ValueTuple<int, int>(1, 1),
            new ValueTuple<int, bool>(1, true),
            new ValueTuple<int, long>(1, 1),
            new ValueTuple<int, float>(1, 1f),
            new ValueTuple<int, double>(1, 1),
            new ValueTuple<int, object>(1, null),
            new ValueTuple<int, IntEnum>(1, IntEnum.A),
            new ValueTuple<long, int>(1, 1),
            new ValueTuple<long, long>(1, 1),
            new ValueTuple<long, float>(1, 1f),
            new ValueTuple<long, double>(1, 1),
            new ValueTuple<long, object>(1, null),
            new ValueTuple<long, IntEnum>(1, IntEnum.A),
            new ValueTuple<float, int>(1f, 1),
            new ValueTuple<float, long>(1f, 1),
            new ValueTuple<float, double>(1f, 1),
            new ValueTuple<float, object>(1f, null),
            new ValueTuple<float, IntEnum>(1f, IntEnum.A),
            new ValueTuple<double, int>(1, 1),
            new ValueTuple<double, long>(1, 1),
            new ValueTuple<double, float>(1, 1f),
            new ValueTuple<double, object>(1, null),
            new ValueTuple<double, IntEnum>(1, IntEnum.A),
            new ValueTuple<object, int>(null, 1),
            new ValueTuple<object, long>(null, 1),
            new ValueTuple<object, float>(null, 1f),
            new ValueTuple<object, double>(null, 1),
            new ValueTuple<object, IntEnum>(null, IntEnum.A),
            new ValueTuple<IntEnum, int>(IntEnum.A, 1),
            new ValueTuple<IntEnum, long>(IntEnum.A, 1),
            new ValueTuple<IntEnum, float>(IntEnum.A, 1f),
            new ValueTuple<IntEnum, double>(IntEnum.A, 1),
            new ValueTuple<IntEnum, object>(IntEnum.A, null),
            new ValueTuple<int, long, float>(1, 1, 1f),
            new ValueTuple<int, long, double>(1, 1, 1),
            new ValueTuple<int, long, object>(1, 1, null),
            new ValueTuple<int, long, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<int, double, int>(1, 1, 1),

            new ValueTuple<int, int, int>(1, 1, 1),
            new ValueTuple<int, long, int>(1, 1, 1),
            new ValueTuple<bool, int>(true, 1),
            new ValueTuple<int, int, int, int>(1, 1, 1, 1),
            new ValueTuple<int, float, long>(1, 1f, 1),
            new ValueTuple<int, float, double>(1, 1f, 1),
            new ValueTuple<int, float, object>(1, 1f, null),
            new ValueTuple<int, float, IntEnum>(1, 1f, IntEnum.A),
            new ValueTuple<int, double, long>(1, 1, 1),
            new ValueTuple<int, double, float>(1, 1, 1f),
            new ValueTuple<int, double, object>(1, 1, null),
            new ValueTuple<int, double, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<int, object, long>(1, null, 1),
            new ValueTuple<int, object, float>(1, null, 1f),
            new ValueTuple<int, object, double>(1, null, 1),
            new ValueTuple<int, object, IntEnum>(1, null, IntEnum.A),
            new ValueTuple<int, IntEnum, long>(1, IntEnum.A, 1),
            new ValueTuple<int, IntEnum, float>(1, IntEnum.A, 1f),
            new ValueTuple<int, IntEnum, double>(1, IntEnum.A, 1),
            new ValueTuple<int, IntEnum, object>(1, IntEnum.A, null),
            new ValueTuple<long, int, float>(1, 1, 1f),
            new ValueTuple<long, int, double>(1, 1, 1),
            new ValueTuple<long, int, object>(1, 1, null),
            new ValueTuple<long, int, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<long, float, int>(1, 1f, 1),
            new ValueTuple<long, float, double>(1, 1f, 1),
            new ValueTuple<long, float, object>(1, 1f, null),
            new ValueTuple<long, float, IntEnum>(1, 1f, IntEnum.A),
            new ValueTuple<long, double, int>(1, 1, 1),
            new ValueTuple<long, double, float>(1, 1, 1f),
            new ValueTuple<long, double, object>(1, 1, null),
            new ValueTuple<long, double, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<long, object, int>(1, null, 1),
            new ValueTuple<long, object, float>(1, null, 1f),
            new ValueTuple<long, object, double>(1, null, 1),
            new ValueTuple<long, object, IntEnum>(1, null, IntEnum.A),
            new ValueTuple<long, IntEnum, int>(1, IntEnum.A, 1),
            new ValueTuple<long, IntEnum, float>(1, IntEnum.A, 1f),
            new ValueTuple<long, IntEnum, double>(1, IntEnum.A, 1),
            new ValueTuple<long, IntEnum, object>(1, IntEnum.A, null),
            new ValueTuple<float, int, long>(1f, 1, 1),
            new ValueTuple<float, int, double>(1f, 1, 1),
            new ValueTuple<float, int, object>(1f, 1, null),
            new ValueTuple<float, int, IntEnum>(1f, 1, IntEnum.A),
            new ValueTuple<float, long, int>(1f, 1, 1),
            new ValueTuple<float, long, double>(1f, 1, 1),
            new ValueTuple<float, long, object>(1f, 1, null),
            new ValueTuple<float, long, IntEnum>(1f, 1, IntEnum.A),
            new ValueTuple<float, double, int>(1f, 1, 1),
            new ValueTuple<float, double, long>(1f, 1, 1),
            new ValueTuple<float, double, object>(1f, 1, null),
            new ValueTuple<float, double, IntEnum>(1f, 1, IntEnum.A),
            new ValueTuple<float, object, int>(1f, null, 1),
            new ValueTuple<float, object, long>(1f, null, 1),
            new ValueTuple<float, object, double>(1f, null, 1),
            new ValueTuple<float, object, IntEnum>(1f, null, IntEnum.A),
            new ValueTuple<float, IntEnum, int>(1f, IntEnum.A, 1),
            new ValueTuple<float, IntEnum, long>(1f, IntEnum.A, 1),
            new ValueTuple<float, IntEnum, double>(1f, IntEnum.A, 1),
            new ValueTuple<float, IntEnum, object>(1f, IntEnum.A, null),
            new ValueTuple<double, int, long>(1, 1, 1),
            new ValueTuple<double, int, float>(1, 1, 1f),
            new ValueTuple<double, int, object>(1, 1, null),
            new ValueTuple<double, int, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<double, long, int>(1, 1, 1),
            new ValueTuple<double, long, float>(1, 1, 1f),
            new ValueTuple<double, long, object>(1, 1, null),
            new ValueTuple<double, long, IntEnum>(1, 1, IntEnum.A),
            new ValueTuple<double, float, int>(1, 1f, 1),
            new ValueTuple<double, float, long>(1, 1f, 1),
            new ValueTuple<double, float, object>(1, 1f, null),
            new ValueTuple<double, float, IntEnum>(1, 1f, IntEnum.A),
            new ValueTuple<double, object, int>(1, null, 1),
            new ValueTuple<double, object, long>(1, null, 1),
            new ValueTuple<double, object, float>(1, null, 1f),
            new ValueTuple<double, object, IntEnum>(1, null, IntEnum.A),
            new ValueTuple<double, IntEnum, int>(1, IntEnum.A, 1),
            new ValueTuple<double, IntEnum, long>(1, IntEnum.A, 1),
            new ValueTuple<double, IntEnum, float>(1, IntEnum.A, 1f),
            new ValueTuple<double, IntEnum, object>(1, IntEnum.A, null),
            new ValueTuple<object, int, long>(null, 1, 1),
            new ValueTuple<object, int, float>(null, 1, 1f),
            new ValueTuple<object, int, double>(null, 1, 1),
            new ValueTuple<object, int, IntEnum>(null, 1, IntEnum.A),
            new ValueTuple<object, long, int>(null, 1, 1),
            new ValueTuple<object, long, float>(null, 1, 1f),
            new ValueTuple<object, long, double>(null, 1, 1),
            new ValueTuple<object, long, IntEnum>(null, 1, IntEnum.A),
            new ValueTuple<object, float, int>(null, 1f, 1),
            new ValueTuple<object, float, long>(null, 1f, 1),
            new ValueTuple<object, float, double>(null, 1f, 1),
            new ValueTuple<object, float, IntEnum>(null, 1f, IntEnum.A),
            new ValueTuple<object, double, int>(null, 1, 1),
            new ValueTuple<object, double, long>(null, 1, 1),
            new ValueTuple<object, double, float>(null, 1, 1f),
            new ValueTuple<object, double, IntEnum>(null, 1, IntEnum.A),
            new ValueTuple<object, IntEnum, int>(null, IntEnum.A, 1),
            new ValueTuple<object, IntEnum, long>(null, IntEnum.A, 1),
            new ValueTuple<object, IntEnum, float>(null, IntEnum.A, 1f),
            new ValueTuple<object, IntEnum, double>(null, IntEnum.A, 1),
            new ValueTuple<IntEnum, int, long>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, int, float>(IntEnum.A, 1, 1f),
            new ValueTuple<IntEnum, int, double>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, int, object>(IntEnum.A, 1, null),
            new ValueTuple<IntEnum, long, int>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, long, float>(IntEnum.A, 1, 1f),
            new ValueTuple<IntEnum, long, double>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, long, object>(IntEnum.A, 1, null),
            new ValueTuple<IntEnum, float, int>(IntEnum.A, 1f, 1),
            new ValueTuple<IntEnum, float, long>(IntEnum.A, 1f, 1),
            new ValueTuple<IntEnum, float, double>(IntEnum.A, 1f, 1),
            new ValueTuple<IntEnum, float, object>(IntEnum.A, 1f, null),
            new ValueTuple<IntEnum, double, int>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, double, long>(IntEnum.A, 1, 1),
            new ValueTuple<IntEnum, double, float>(IntEnum.A, 1, 1f),
            new ValueTuple<IntEnum, double, object>(IntEnum.A, 1, null),
            new ValueTuple<IntEnum, object, int>(IntEnum.A, null, 1),
            new ValueTuple<IntEnum, object, long>(IntEnum.A, null, 1),
            new ValueTuple<IntEnum, object, float>(IntEnum.A, null, 1f),
            new ValueTuple<IntEnum, object, double>(IntEnum.A, null, 1),
            new TaskAwaiter<int>(),
            new TaskAwaiter<float>(),
            new TaskAwaiter<long>(),
            new TaskAwaiter<IntEnum>(),
            new TaskAwaiter<object>(),
            new TaskAwaiter<string>(),
            new TaskAwaiter<double>(),
            new TaskAwaiter<List<object>>(),
            
                new Dictionary<int, long>(),
                new Dictionary<int, float>(),
                new Dictionary<int, double>(),
                new Dictionary<int, object>(),
                new Dictionary<int, IntEnum>(),
                new Dictionary<long, int>(),
                new Dictionary<long, float>(),
                new Dictionary<long, double>(),
                new Dictionary<long, object>(),
                new Dictionary<long, IntEnum>(),
                new Dictionary<float, int>(),
                new Dictionary<float, long>(),
                new Dictionary<float, double>(),
                new Dictionary<float, object>(),
                new Dictionary<float, IntEnum>(),
                new Dictionary<double, int>(),
                new Dictionary<double, long>(),
                new Dictionary<double, float>(),
                new Dictionary<double, object>(),
                new Dictionary<double, IntEnum>(),
                new Dictionary<object, int>(),
                new Dictionary<object, long>(),
                new Dictionary<object, float>(),
                new Dictionary<object, double>(),
                new Dictionary<object, IntEnum>(),
                new Dictionary<IntEnum, int>(),
                new Dictionary<IntEnum, long>(),
                new Dictionary<IntEnum, float>(),
                new Dictionary<IntEnum, double>(),
                new Dictionary<IntEnum, object>(),
                new Dictionary<(int, long), object>(),
                new Dictionary<(int, float), object>(),
                new Dictionary<(int, double), object>(),
                new Dictionary<(int, object), object>(),
                new Dictionary<(int, IntEnum), object>(),
                new Dictionary<(long, int), object>(),
                new Dictionary<(long, float), object>(),
                new Dictionary<(long, double), object>(),
                new Dictionary<(long, object), object>(),
                new Dictionary<(long, IntEnum), object>(),
                new Dictionary<(float, int), object>(),
                new Dictionary<(float, long), object>(),
                new Dictionary<(float, double), object>(),
                new Dictionary<(float, object), object>(),
                new Dictionary<(float, IntEnum), object>(),
                new Dictionary<(double, int), object>(),
                new Dictionary<(double, long), object>(),
                new Dictionary<(double, float), object>(),
                new Dictionary<(double, object), object>(),
                new Dictionary<(double, IntEnum), object>(),
                new Dictionary<(object, int), object>(),
                new Dictionary<(object, long), object>(),
                new Dictionary<(object, float), object>(),
                new Dictionary<(object, double), object>(),
                new Dictionary<(object, IntEnum), object>(),
                new Dictionary<(IntEnum, int), object>(),
                new Dictionary<(IntEnum, long), object>(),
                new Dictionary<(IntEnum, float), object>(),
                new Dictionary<(IntEnum, double), object>(),
                new Dictionary<(IntEnum, object), object>(),
                new Dictionary<(int, long, float), object>(),
                new Dictionary<(int, long, double), object>(),
                new Dictionary<(int, long, object), object>(),
                new Dictionary<(int, long, IntEnum), object>(),
                new Dictionary<(int, float, long), object>(),
                new Dictionary<(int, float, double), object>(),
                new Dictionary<(int, float, object), object>(),
                new Dictionary<(int, float, IntEnum), object>(),
                new Dictionary<(int, double, long), object>(),
                new Dictionary<(int, double, float), object>(),
                new Dictionary<(int, double, object), object>(),
                new Dictionary<(int, double, IntEnum), object>(),
                new Dictionary<(int, object, long), object>(),
                new Dictionary<(int, object, float), object>(),
                new Dictionary<(int, object, double), object>(),
                new Dictionary<(int, object, IntEnum), object>(),
                new Dictionary<(int, IntEnum, long), object>(),
                new Dictionary<(int, IntEnum, float), object>(),
                new Dictionary<(int, IntEnum, double), object>(),
                new Dictionary<(int, IntEnum, object), object>(),
                new Dictionary<(long, int, float), object>(),
                new Dictionary<(long, int, double), object>(),
                new Dictionary<(long, int, object), object>(),
                new Dictionary<(long, int, IntEnum), object>(),
                new Dictionary<(long, float, int), object>(),
                new Dictionary<(long, float, double), object>(),
                new Dictionary<(long, float, object), object>(),
                new Dictionary<(long, float, IntEnum), object>(),
                new Dictionary<(long, double, int), object>(),
                new Dictionary<(long, double, float), object>(),
                new Dictionary<(long, double, object), object>(),
                new Dictionary<(long, double, IntEnum), object>(),
                new Dictionary<(long, object, int), object>(),
                new Dictionary<(long, object, float), object>(),
                new Dictionary<(long, object, double), object>(),
                new Dictionary<(long, object, IntEnum), object>(),
                new Dictionary<(long, IntEnum, int), object>(),
                new Dictionary<(long, IntEnum, float), object>(),
                new Dictionary<(long, IntEnum, double), object>(),
                new Dictionary<(long, IntEnum, object), object>(),
                new Dictionary<(float, int, long), object>(),
                new Dictionary<(float, int, double), object>(),
                new Dictionary<(float, int, object), object>(),
                new Dictionary<(float, int, IntEnum), object>(),
                new Dictionary<(float, long, int), object>(),
                new Dictionary<(float, long, double), object>(),
                new Dictionary<(float, long, object), object>(),
                new Dictionary<(float, long, IntEnum), object>(),
                new Dictionary<(float, double, int), object>(),
                new Dictionary<(float, double, long), object>(),
                new Dictionary<(float, double, object), object>(),
                new Dictionary<(float, double, IntEnum), object>(),
                new Dictionary<(float, object, int), object>(),
                new Dictionary<(float, object, long), object>(),
                new Dictionary<(float, object, double), object>(),
                new Dictionary<(float, object, IntEnum), object>(),
                new Dictionary<(float, IntEnum, int), object>(),
                new Dictionary<(float, IntEnum, long), object>(),
                new Dictionary<(float, IntEnum, double), object>(),
                new Dictionary<(float, IntEnum, object), object>(),
                new Dictionary<(double, int, long), object>(),
                new Dictionary<(double, int, float), object>(),
                new Dictionary<(double, int, object), object>(),
                new Dictionary<(double, int, IntEnum), object>(),
                new Dictionary<(double, long, int), object>(),
                new Dictionary<(double, long, float), object>(),
                new Dictionary<(double, long, object), object>(),
                new Dictionary<(double, long, IntEnum), object>(),
                new Dictionary<(double, float, int), object>(),
                new Dictionary<(double, float, long), object>(),
                new Dictionary<(double, float, object), object>(),
                new Dictionary<(double, float, IntEnum), object>(),
                new Dictionary<(double, object, int), object>(),
                new Dictionary<(double, object, long), object>(),
                new Dictionary<(double, object, float), object>(),
                new Dictionary<(double, object, IntEnum), object>(),
                new Dictionary<(double, IntEnum, int), object>(),
                new Dictionary<(double, IntEnum, long), object>(),
                new Dictionary<(double, IntEnum, float), object>(),
                new Dictionary<(double, IntEnum, object), object>(),
                new Dictionary<(object, int, long), object>(),
                new Dictionary<(object, int, float), object>(),
                new Dictionary<(object, int, double), object>(),
                new Dictionary<(object, int, IntEnum), object>(),
                new Dictionary<(object, long, int), object>(),
                new Dictionary<(object, long, float), object>(),
                new Dictionary<(object, long, double), object>(),
                new Dictionary<(object, long, IntEnum), object>(),
                new Dictionary<(object, float, int), object>(),
                new Dictionary<(object, float, long), object>(),
                new Dictionary<(object, float, double), object>(),
                new Dictionary<(object, float, IntEnum), object>(),
                new Dictionary<(object, double, int), object>(),
                new Dictionary<(object, double, long), object>(),
                new Dictionary<(object, double, float), object>(),
                new Dictionary<(object, double, IntEnum), object>(),
                new Dictionary<(object, IntEnum, int), object>(),
                new Dictionary<(object, IntEnum, long), object>(),
                new Dictionary<(object, IntEnum, float), object>(),
                new Dictionary<(object, IntEnum, double), object>(),
                new Dictionary<(IntEnum, int, long), object>(),
                new Dictionary<(IntEnum, int, float), object>(),
                new Dictionary<(IntEnum, int, double), object>(),
                new Dictionary<(IntEnum, int, object), object>(),
                new Dictionary<(IntEnum, long, int), object>(),
                new Dictionary<(IntEnum, long, float), object>(),
                new Dictionary<(IntEnum, long, double), object>(),
                new Dictionary<(IntEnum, long, object), object>(),
                new Dictionary<(IntEnum, float, int), object>(),
                new Dictionary<(IntEnum, float, long), object>(),
                new Dictionary<(IntEnum, float, double), object>(),
                new Dictionary<(IntEnum, float, object), object>(),
                new Dictionary<(IntEnum, double, int), object>(),
                new Dictionary<(IntEnum, double, long), object>(),
                new Dictionary<(IntEnum, double, float), object>(),
                new Dictionary<(IntEnum, double, object), object>(),
                new Dictionary<(IntEnum, object, int), object>(),
                new Dictionary<(IntEnum, object, long), object>(),
                new Dictionary<(IntEnum, object, float), object>(),
                new Dictionary<(IntEnum, object, double), object>(),
                new SortedDictionary<int, long>(),
                new SortedDictionary<int, float>(),
                new SortedDictionary<int, double>(),
                new SortedDictionary<int, object>(),
                new SortedDictionary<int, IntEnum>(),
                new SortedDictionary<long, int>(),
                new SortedDictionary<long, float>(),
                new SortedDictionary<long, double>(),
                new SortedDictionary<long, object>(),
                new SortedDictionary<long, IntEnum>(),
                new SortedDictionary<float, int>(),
                new SortedDictionary<float, long>(),
                new SortedDictionary<float, double>(),
                new SortedDictionary<float, object>(),
                new SortedDictionary<float, IntEnum>(),
                new SortedDictionary<double, int>(),
                new SortedDictionary<double, long>(),
                new SortedDictionary<double, float>(),
                new SortedDictionary<double, object>(),
                new SortedDictionary<double, IntEnum>(),
                new SortedDictionary<object, int>(),
                new SortedDictionary<object, long>(),
                new SortedDictionary<object, float>(),
                new SortedDictionary<object, double>(),
                new SortedDictionary<object, IntEnum>(),
                new SortedDictionary<IntEnum, int>(),
                new SortedDictionary<IntEnum, long>(),
                new SortedDictionary<IntEnum, float>(),
                new SortedDictionary<IntEnum, double>(),
                new SortedDictionary<IntEnum, object>(),
                new HashSet<int>(),
                new HashSet<long>(),
                new HashSet<float>(),
                new HashSet<double>(),
                new HashSet<object>(),
                new HashSet<IntEnum>(),
                new List<int>(),
                new List<long>(),
                new List<float>(),
                new List<double>(),
                new List<object>(),
                new List<IntEnum>(),
                new ValueTuple<int>(1),
                new ValueTuple<long>(1),
                new ValueTuple<float>(1f),
                new ValueTuple<double>(1),
                new ValueTuple<object>(null),
                new ValueTuple<IntEnum>(IntEnum.A),
                new ValueTuple<int, long>(1, 1),
                new ValueTuple<int, float>(1, 1f),
                new ValueTuple<int, double>(1, 1),
                new ValueTuple<int, object>(1, null),
                new ValueTuple<int, IntEnum>(1, IntEnum.A),
                new ValueTuple<long, int>(1, 1),
                new ValueTuple<long, float>(1, 1f),
                new ValueTuple<long, double>(1, 1),
                new ValueTuple<long, object>(1, null),
                new ValueTuple<long, IntEnum>(1, IntEnum.A),
                new ValueTuple<float, int>(1f, 1),
                new ValueTuple<float, long>(1f, 1),
                new ValueTuple<float, double>(1f, 1),
                new ValueTuple<float, object>(1f, null),
                new ValueTuple<float, IntEnum>(1f, IntEnum.A),
                new ValueTuple<double, int>(1, 1),
                new ValueTuple<double, long>(1, 1),
                new ValueTuple<double, float>(1, 1f),
                new ValueTuple<double, object>(1, null),
                new ValueTuple<double, IntEnum>(1, IntEnum.A),
                new ValueTuple<object, int>(null, 1),
                new ValueTuple<object, long>(null, 1),
                new ValueTuple<object, float>(null, 1f),
                new ValueTuple<object, double>(null, 1),
                new ValueTuple<object, IntEnum>(null, IntEnum.A),
                new ValueTuple<IntEnum, int>(IntEnum.A, 1),
                new ValueTuple<IntEnum, long>(IntEnum.A, 1),
                new ValueTuple<IntEnum, float>(IntEnum.A, 1f),
                new ValueTuple<IntEnum, double>(IntEnum.A, 1),
                new ValueTuple<IntEnum, object>(IntEnum.A, null),
                new ValueTuple<int, long, float>(1, 1, 1f),
                new ValueTuple<int, long, double>(1, 1, 1),
                new ValueTuple<int, long, object>(1, 1, null),
                new ValueTuple<int, long, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<int, float, long>(1, 1f, 1),
                new ValueTuple<int, float, double>(1, 1f, 1),
                new ValueTuple<int, float, object>(1, 1f, null),
                new ValueTuple<int, float, IntEnum>(1, 1f, IntEnum.A),
                new ValueTuple<int, double, long>(1, 1, 1),
                new ValueTuple<int, double, float>(1, 1, 1f),
                new ValueTuple<int, double, object>(1, 1, null),
                new ValueTuple<int, double, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<int, object, long>(1, null, 1),
                new ValueTuple<int, object, float>(1, null, 1f),
                new ValueTuple<int, object, double>(1, null, 1),
                new ValueTuple<int, object, IntEnum>(1, null, IntEnum.A),
                new ValueTuple<int, IntEnum, long>(1, IntEnum.A, 1),
                new ValueTuple<int, IntEnum, float>(1, IntEnum.A, 1f),
                new ValueTuple<int, IntEnum, double>(1, IntEnum.A, 1),
                new ValueTuple<int, IntEnum, object>(1, IntEnum.A, null),
                new ValueTuple<long, int, float>(1, 1, 1f),
                new ValueTuple<long, int, double>(1, 1, 1),
                new ValueTuple<long, int, object>(1, 1, null),
                new ValueTuple<long, int, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<long, float, int>(1, 1f, 1),
                new ValueTuple<long, float, double>(1, 1f, 1),
                new ValueTuple<long, float, object>(1, 1f, null),
                new ValueTuple<long, float, IntEnum>(1, 1f, IntEnum.A),
                new ValueTuple<long, double, int>(1, 1, 1),
                new ValueTuple<long, double, float>(1, 1, 1f),
                new ValueTuple<long, double, object>(1, 1, null),
                new ValueTuple<long, double, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<long, object, int>(1, null, 1),
                new ValueTuple<long, object, float>(1, null, 1f),
                new ValueTuple<long, object, double>(1, null, 1),
                new ValueTuple<long, object, IntEnum>(1, null, IntEnum.A),
                new ValueTuple<long, IntEnum, int>(1, IntEnum.A, 1),
                new ValueTuple<long, IntEnum, float>(1, IntEnum.A, 1f),
                new ValueTuple<long, IntEnum, double>(1, IntEnum.A, 1),
                new ValueTuple<long, IntEnum, object>(1, IntEnum.A, null),
                new ValueTuple<float, int, long>(1f, 1, 1),
                new ValueTuple<float, int, double>(1f, 1, 1),
                new ValueTuple<float, int, object>(1f, 1, null),
                new ValueTuple<float, int, IntEnum>(1f, 1, IntEnum.A),
                new ValueTuple<float, long, int>(1f, 1, 1),
                new ValueTuple<float, long, double>(1f, 1, 1),
                new ValueTuple<float, long, object>(1f, 1, null),
                new ValueTuple<float, long, IntEnum>(1f, 1, IntEnum.A),
                new ValueTuple<float, double, int>(1f, 1, 1),
                new ValueTuple<float, double, long>(1f, 1, 1),
                new ValueTuple<float, double, object>(1f, 1, null),
                new ValueTuple<float, double, IntEnum>(1f, 1, IntEnum.A),
                new ValueTuple<float, object, int>(1f, null, 1),
                new ValueTuple<float, object, long>(1f, null, 1),
                new ValueTuple<float, object, double>(1f, null, 1),
                new ValueTuple<float, object, IntEnum>(1f, null, IntEnum.A),
                new ValueTuple<float, IntEnum, int>(1f, IntEnum.A, 1),
                new ValueTuple<float, IntEnum, long>(1f, IntEnum.A, 1),
                new ValueTuple<float, IntEnum, double>(1f, IntEnum.A, 1),
                new ValueTuple<float, IntEnum, object>(1f, IntEnum.A, null),
                new ValueTuple<double, int, long>(1, 1, 1),
                new ValueTuple<double, int, float>(1, 1, 1f),
                new ValueTuple<double, int, object>(1, 1, null),
                new ValueTuple<double, int, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<double, long, int>(1, 1, 1),
                new ValueTuple<double, long, float>(1, 1, 1f),
                new ValueTuple<double, long, object>(1, 1, null),
                new ValueTuple<double, long, IntEnum>(1, 1, IntEnum.A),
                new ValueTuple<double, float, int>(1, 1f, 1),
                new ValueTuple<double, float, long>(1, 1f, 1),
                new ValueTuple<double, float, object>(1, 1f, null),
                new ValueTuple<double, float, IntEnum>(1, 1f, IntEnum.A),
                new ValueTuple<double, object, int>(1, null, 1),
                new ValueTuple<double, object, long>(1, null, 1),
                new ValueTuple<double, object, float>(1, null, 1f),
                new ValueTuple<double, object, IntEnum>(1, null, IntEnum.A),
                new ValueTuple<double, IntEnum, int>(1, IntEnum.A, 1),
                new ValueTuple<double, IntEnum, long>(1, IntEnum.A, 1),
                new ValueTuple<double, IntEnum, float>(1, IntEnum.A, 1f),
                new ValueTuple<double, IntEnum, object>(1, IntEnum.A, null),
                new ValueTuple<object, int, long>(null, 1, 1),
                new ValueTuple<object, int, float>(null, 1, 1f),
                new ValueTuple<object, int, double>(null, 1, 1),
                new ValueTuple<object, int, IntEnum>(null, 1, IntEnum.A),
                new ValueTuple<object, long, int>(null, 1, 1),
                new ValueTuple<object, long, float>(null, 1, 1f),
                new ValueTuple<object, long, double>(null, 1, 1),
                new ValueTuple<object, long, IntEnum>(null, 1, IntEnum.A),
                new ValueTuple<object, float, int>(null, 1f, 1),
                new ValueTuple<object, float, long>(null, 1f, 1),
                new ValueTuple<object, float, double>(null, 1f, 1),
                new ValueTuple<object, float, IntEnum>(null, 1f, IntEnum.A),
                new ValueTuple<object, double, int>(null, 1, 1),
                new ValueTuple<object, double, long>(null, 1, 1),
                new ValueTuple<object, double, float>(null, 1, 1f),
                new ValueTuple<object, double, IntEnum>(null, 1, IntEnum.A),
                new ValueTuple<object, IntEnum, int>(null, IntEnum.A, 1),
                new ValueTuple<object, IntEnum, long>(null, IntEnum.A, 1),
                new ValueTuple<object, IntEnum, float>(null, IntEnum.A, 1f),
                new ValueTuple<object, IntEnum, double>(null, IntEnum.A, 1),
                new ValueTuple<IntEnum, int, long>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, int, float>(IntEnum.A, 1, 1f),
                new ValueTuple<IntEnum, int, double>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, int, object>(IntEnum.A, 1, null),
                new ValueTuple<IntEnum, long, int>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, long, float>(IntEnum.A, 1, 1f),
                new ValueTuple<IntEnum, long, double>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, long, object>(IntEnum.A, 1, null),
                new ValueTuple<IntEnum, float, int>(IntEnum.A, 1f, 1),
                new ValueTuple<IntEnum, float, long>(IntEnum.A, 1f, 1),
                new ValueTuple<IntEnum, float, double>(IntEnum.A, 1f, 1),
                new ValueTuple<IntEnum, float, object>(IntEnum.A, 1f, null),
                new ValueTuple<IntEnum, double, int>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, double, long>(IntEnum.A, 1, 1),
                new ValueTuple<IntEnum, double, float>(IntEnum.A, 1, 1f),
                new ValueTuple<IntEnum, double, object>(IntEnum.A, 1, null),
                new ValueTuple<IntEnum, object, int>(IntEnum.A, null, 1),
                new ValueTuple<IntEnum, object, long>(IntEnum.A, null, 1),
                new ValueTuple<IntEnum, object, float>(IntEnum.A, null, 1f),
                new ValueTuple<IntEnum, object, double>(IntEnum.A, null, 1)
            };
        }
        
        
        class RefStateMachine : IAsyncStateMachine
        {
            public void MoveNext()
            {
                throw new NotImplementedException();
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                throw new NotImplementedException();
            }
        }

        void RefAsyncMethod()
        {
            
            #region ASYNC
        var stateMachine = new RefStateMachine();

        TaskAwaiter wnull = default;
        TaskAwaiter<bool> wbool = default;
        TaskAwaiter<int> wint = default;
        TaskAwaiter<long> wlong = default;
        TaskAwaiter<object> wobject = default;
        TaskAwaiter<string> wstring = default;
        TaskAwaiter<double> wdouble = default;
        TaskAwaiter<float> wfloat = default;
        TaskAwaiter<IntEnum> wenum = default;

        var bnull = new AsyncTaskMethodBuilder();
        var bbool = new AsyncTaskMethodBuilder<bool>();
        var bint = new AsyncTaskMethodBuilder<int>();
        var bobject = new AsyncTaskMethodBuilder<object>();
        var bstring = new AsyncTaskMethodBuilder<string>();
        var bdouble = new AsyncTaskMethodBuilder<double>();
        var bfloat = new AsyncTaskMethodBuilder<float>();
        var benum = new AsyncTaskMethodBuilder<IntEnum>();
        var vbnull = new AsyncVoidMethodBuilder();

        vbnull.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        vbnull.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wnull, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wint, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wobject, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wstring, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wdouble, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wfloat, ref stateMachine);
        vbnull.AwaitOnCompleted(ref wenum, ref stateMachine);
        vbnull.SetException(null);
        vbnull.SetResult();


        bnull.Start(ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bnull.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bnull.AwaitOnCompleted(ref wnull, ref stateMachine);
        bnull.AwaitOnCompleted(ref wint, ref stateMachine);
        bnull.AwaitOnCompleted(ref wobject, ref stateMachine);
        bnull.AwaitOnCompleted(ref wstring, ref stateMachine);
        bnull.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bnull.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bnull.AwaitOnCompleted(ref wenum, ref stateMachine);
        bnull.SetException(null);
        bnull.SetResult();

        bbool.Start(ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bbool.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bbool.AwaitOnCompleted(ref wnull, ref stateMachine);
        bbool.AwaitOnCompleted(ref wint, ref stateMachine);
        bbool.AwaitOnCompleted(ref wobject, ref stateMachine);
        bbool.AwaitOnCompleted(ref wstring, ref stateMachine);
        bbool.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bbool.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bbool.AwaitOnCompleted(ref wenum, ref stateMachine);
        bbool.SetException(null);
        bbool.SetResult(default);

        bint.Start(ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bint.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bint.AwaitOnCompleted(ref wnull, ref stateMachine);
        bint.AwaitOnCompleted(ref wint, ref stateMachine);
        bint.AwaitOnCompleted(ref wobject, ref stateMachine);
        bint.AwaitOnCompleted(ref wstring, ref stateMachine);
        bint.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bint.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bint.AwaitOnCompleted(ref wenum, ref stateMachine);
        bint.SetException(null);
        bint.SetResult(default);

        bobject.Start(ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bobject.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bobject.AwaitOnCompleted(ref wnull, ref stateMachine);
        bobject.AwaitOnCompleted(ref wint, ref stateMachine);
        bobject.AwaitOnCompleted(ref wobject, ref stateMachine);
        bobject.AwaitOnCompleted(ref wstring, ref stateMachine);
        bobject.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bobject.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bobject.AwaitOnCompleted(ref wenum, ref stateMachine);
        bobject.SetException(null);
        bobject.SetResult(default);

        bstring.Start(ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bstring.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bstring.AwaitOnCompleted(ref wnull, ref stateMachine);
        bstring.AwaitOnCompleted(ref wint, ref stateMachine);
        bstring.AwaitOnCompleted(ref wobject, ref stateMachine);
        bstring.AwaitOnCompleted(ref wstring, ref stateMachine);
        bstring.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bstring.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bstring.AwaitOnCompleted(ref wenum, ref stateMachine);
        bstring.SetException(null);
        bstring.SetResult(default);

        bdouble.Start(ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bdouble.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wnull, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wint, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wobject, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wstring, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bdouble.AwaitOnCompleted(ref wenum, ref stateMachine);
        bdouble.SetException(null);
        bdouble.SetResult(default);

        bfloat.Start(ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        bfloat.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wnull, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wint, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wobject, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wstring, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wdouble, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wfloat, ref stateMachine);
        bfloat.AwaitOnCompleted(ref wenum, ref stateMachine);
        bfloat.SetException(null);
        bfloat.SetResult(default);

        benum.Start(ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wnull, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wbool, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wint, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wlong, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wobject, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wstring, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wdouble, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wfloat, ref stateMachine);
        benum.AwaitUnsafeOnCompleted(ref wenum, ref stateMachine);
        benum.AwaitOnCompleted(ref wnull, ref stateMachine);
        benum.AwaitOnCompleted(ref wint, ref stateMachine);
        benum.AwaitOnCompleted(ref wobject, ref stateMachine);
        benum.AwaitOnCompleted(ref wstring, ref stateMachine);
        benum.AwaitOnCompleted(ref wdouble, ref stateMachine);
        benum.AwaitOnCompleted(ref wfloat, ref stateMachine);
        benum.AwaitOnCompleted(ref wenum, ref stateMachine);
        benum.SetException(null);
        benum.SetResult(default);
#endregion

// var stateMachine2 = new RefStateMachine();

            
            TaskAwaiter aw = default;
            var c0 = new AsyncTaskMethodBuilder();
            c0.Start(ref stateMachine);
            c0.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c0.SetException(null);
            c0.SetResult();

            // // by Ron
            // TaskAwaiter<bool> awBool = new TaskAwaiter<bool>();
            // var cRon = new AsyncTaskMethodBuilder();
            // cRon.AwaitUnsafeOnCompleted(ref awBool, ref stateMachine);
            // cRon.SetException(null);
            // cRon.SetResult();

            // by Ron
            TaskAwaiter<bool> awBool2 = new TaskAwaiter<bool>();
            var cRon2 = new AsyncTaskMethodBuilder<TaskAwaiter<bool>>();
            cRon2.Start(ref stateMachine);
            cRon2.AwaitUnsafeOnCompleted(ref awBool2, ref stateMachine);
            cRon2.SetException(null);
            cRon2.SetResult(default);

            var cRon3 = System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create();
            cRon3.Start(ref stateMachine);
            cRon3.AwaitUnsafeOnCompleted(ref awBool2, ref stateMachine);
            cRon3.SetException(null);
            cRon3.SetResult();

            // TaskAwaiter<KeyValuePair<System.Net.HttpStatusCode, LitJson.JsonData>> awTmp1 = new TaskAwaiter<KeyValuePair<System.Net.HttpStatusCode, LitJson.JsonData>>();
            // var cRon4 = System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create();
            // cRon4.Start(ref stateMachine);
            // cRon4.AwaitUnsafeOnCompleted(ref awTmp1, ref stateMachine);
            // cRon4.SetException(null);
            // cRon4.SetResult();

            TaskAwaiter<object> awObj = new TaskAwaiter<object>();
            var cRon5 = System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create();
            cRon5.Start(ref stateMachine);
            cRon5.AwaitUnsafeOnCompleted(ref awObj, ref stateMachine);
            cRon5.SetException(null);
            cRon5.SetResult();
            
            TaskAwaiter<int> awObj51 = new TaskAwaiter<int>();
            var cRon51 = System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create();
            cRon51.Start(ref stateMachine);
            cRon51.AwaitUnsafeOnCompleted(ref awObj51, ref stateMachine);
            cRon51.SetException(null);
            cRon51.SetResult();

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter`1[[BPAB.VersionConfig, BPAB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPAB.BPABManager_v3+<_GetABIndexFromServer>d__24>(System.Runtime.CompilerServices.TaskAwaiter`1[[BPAB.VersionConfig, BPAB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPAB.BPABManager_v3+<_GetABIndexFromServer>d__24)
            TaskAwaiter<object> awObj6 = new TaskAwaiter<object>();
            var cRon6 = new AsyncTaskMethodBuilder<bool>();
            cRon6.Start(ref stateMachine);
            cRon6.AwaitUnsafeOnCompleted(ref awObj6, ref stateMachine);
            cRon6.AwaitOnCompleted(ref awObj6, ref stateMachine);
            cRon6.SetException(null);
            cRon6.SetResult(default);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter`1[[System.Collections.Generic.KeyValuePair`2[[System.Net.HttpStatusCode, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[LitJson.JsonData, LitJson, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]],BPA
            TaskAwaiter<KeyValuePair<IntEnum, object>> ronAwait7 = new TaskAwaiter<KeyValuePair<IntEnum, object>>();
            var cRon7 = new AsyncTaskMethodBuilder<bool>();
            cRon7.Start(ref stateMachine);
            cRon7.AwaitUnsafeOnCompleted(ref ronAwait7, ref stateMachine);
            cRon7.SetException(null);
            cRon7.SetResult(default);
            
            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter`1[[System.Collections.Generic.KeyValuePair`2[[System.Net.HttpStatusCode, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[LitJson.JsonData, LitJson, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]],BPA
            TaskAwaiter<object> ronAwait71 = new TaskAwaiter<object>();
            var cRon71 = new AsyncVoidMethodBuilder();
            cRon71.Start(ref stateMachine);
            cRon71.AwaitUnsafeOnCompleted(ref ronAwait71, ref stateMachine);
            cRon71.SetException(null);
            cRon71.SetResult();

            // AOT generic method isn't instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[BPAB.VersionConfig, BPAB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter`1[[BPAB.VersionConfig, BPAB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPAB.BPABDownloadVersionConfig+<GetLocalVersionConfigFromHotfixDir>d__2>(System.Runtime.CompilerServices.TaskAwaiter`1[[BPAB.VersionConfig, BPAB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],BPAB.BPABDownloadVersionConfig+<GetLoca
            TaskAwaiter<object> awObj8 = new TaskAwaiter<object>();
            var cRon8 = new AsyncTaskMethodBuilder<object>();
            cRon8.Start(ref stateMachine);
            cRon8.AwaitUnsafeOnCompleted(ref awObj8, ref stateMachine);
            cRon8.SetException(null);
            cRon8.SetResult(default);

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Collections.Generic.KeyValuePair`2[[System.Net.HttpStatusCode, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[LitJson.JsonData, LitJson, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Collections.Generic.KeyValuePair`2[[System.Net.HttpStatusCode, System, Version=4.0.0.0, Culture=neutral, PublicKeyT
            TaskAwaiter<KeyValuePair<IntEnum, object>> ronAwait9 = new TaskAwaiter<KeyValuePair<IntEnum, object>>();
            TaskAwaiter<IntEnum> ronAwait91 = new TaskAwaiter<IntEnum>();
            TaskAwaiter<int> ronAwait92 = new TaskAwaiter<int>();
            TaskAwaiter<object> ronAwait93 = new TaskAwaiter<object>();
            var cRon9 = new AsyncTaskMethodBuilder<KeyValuePair<IntEnum, object>>();
            cRon9.Start(ref stateMachine);
            cRon9.AwaitUnsafeOnCompleted(ref ronAwait9, ref stateMachine);
            cRon9.AwaitUnsafeOnCompleted(ref ronAwait91, ref stateMachine);
            cRon9.AwaitUnsafeOnCompleted(ref ronAwait92, ref stateMachine);
            cRon9.AwaitUnsafeOnCompleted(ref ronAwait93, ref stateMachine);
            cRon9.SetException(null);
            cRon9.SetResult(default);

            

            // 问了一下huatuo作者,他说可以用其他类来替代.真机测试通过.
            // MissingMethodException: AOT generic method not instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.Boolean]::AwaitOnCompleted<BPGames.UnityWebRequestAwaiter,BPGames.BPNetworkManagerHelper+NetWorkTask+<Do>d__12>(BPGames.UnityWebRequestAwaiter,BPGames.BPNetworkManagerHelper+NetWorkTask+<Do>d__12)
            // BPMain.UnityWebRequestAwaiter awObj10_1 = new BPMain.UnityWebRequestAwaiter(new UnityEngine.Networking.UnityWebRequestAsyncOperation());
            // TaskAwaiter<object> awObj10 = new TaskAwaiter<object>();
            // var cRon10 = new AsyncTaskMethodBuilder<bool>();
            // cRon10.Start(ref stateMachine);
            // cRon10.AwaitOnCompleted(ref awObj10, ref stateMachine);
            // cRon10.AwaitOnCompleted(ref awObj10_1, ref stateMachine);
            // cRon10.AwaitUnsafeOnCompleted(ref awObj10, ref stateMachine);
            // cRon10.SetException(null);
            // cRon10.SetResult(default);
            

            // MissingMethodException: AOT generic method isn't instantiated in aot module. System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[[System.Collections.Generic.KeyValuePair`2[[System.Net.HttpStatusCode, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[LitJson.JsonData, LitJson, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::Start<BPAB.BPBaseRequest+<SendAsync_ResponseJsonData>d__42>(BPAB.BPBaseRequest+<SendAsync_ResponseJsonData>d__42)

            // BPGames.UnityWebRequestAwaiter bpWwaiter = new BPGames.UnityWebRequestAwaiter(new UnityEngine.Networking.UnityWebRequestAsyncOperation());
            // var cRon4 = System.Runtime.CompilerServices.AsyncTaskMethodBuilder<bool>.Create();
            // cRon4.AwaitOnCompleted(ref bpWwaiter, ref stateMachine);
            // cRon4.SetException(null);
            // cRon4.SetResult(default);


            var c1 = new AsyncTaskMethodBuilder();
            c1.Start(ref stateMachine);
            c1.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c1.SetException(null);
            c1.SetResult();

            var c2 = new AsyncTaskMethodBuilder<bool>();
            c2.Start(ref stateMachine);
            c2.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c2.SetException(null);
            c2.SetResult(default);

            var c3 = new AsyncTaskMethodBuilder<int>();
            c3.Start(ref stateMachine);
            c3.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c3.SetException(null);
            c3.SetResult(default);

            var c4 = new AsyncTaskMethodBuilder<long>();
            c4.Start(ref stateMachine);
            c4.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c4.SetException(null);

            var c5 = new AsyncTaskMethodBuilder<float>();
            c5.Start(ref stateMachine);
            c5.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c5.SetException(null);
            c5.SetResult(default);

            var c6 = new AsyncTaskMethodBuilder<double>();
            c6.Start(ref stateMachine);
            c6.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c6.SetException(null);
            c6.SetResult(default);

            var c7 = new AsyncTaskMethodBuilder<object>();
            c7.Start(ref stateMachine);
            c7.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c7.SetException(null);
            c7.SetResult(default);

            var c8 = new AsyncTaskMethodBuilder<IntEnum>();
            c8.Start(ref stateMachine);
            c8.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c8.SetException(null);
            c8.SetResult(default);

            var c9 = new AsyncVoidMethodBuilder();
            var b = AsyncVoidMethodBuilder.Create();
            c9.Start(ref stateMachine);
            c9.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
            c9.SetException(null);
            c9.SetResult();
            
            object s1 = new object[] {
                new TaskCompletionSource<bool>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<object>(),
                new TaskCompletionSource<IntEnum>(),
            };
            Debug.Log(b);
        }
        public static async void TestAsync3()
        {
            Debug.Log("async task 1");
            await Task.Delay(10);
            Debug.Log("async task 2");
        }
        
        public static int Main_1()
        {
            Debug.Log("hello,huatuo");

            var task = Task.Run(async () =>
            {
                await TestAsync2();
            });

            task.Wait();

            Debug.Log("async task end");
            Debug.Log("async task end2");

            return 0;
        }

        public static async Task TestAsync2()
        {
            Debug.Log("async task 1");
            await Task.Delay(3000);
            Debug.Log("async task 2");
        }

        // Update is called once per frame
   

        public static int TestAsync1()
        {
            var t0 = Task.Run(async () =>
            {
                await Task.Delay(10);
            });
            t0.Wait();
            var task = Task.Run(async () =>
            {
                await Task.Delay(10);
                return 100;
            });
            Debug.Log(task.Result);
            return 0;
        }
        
        // Update is called once per frame

        public static int TestAsync()
        {
            var t0 = Task.Run(async () =>
            {
                await Task.Delay(10);
            });
            t0.Wait();
            var task = Task.Run(async () =>
            {
                await Task.Delay(10);
                return 100;
            });
            Debug.Log(task.Result);
            return 0;
        }
    }
}