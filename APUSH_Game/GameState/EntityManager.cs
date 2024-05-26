using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace APUSH_Game.GameState
{
    internal class EntityManager
    {
        private readonly List<IComponent[]> _entitiesUpdate = new();
        private readonly List<IComponent[]> _entitiesDraw = new();
        private readonly Stack<int> _ids = new();
        private readonly Stack<int> _toDelete = new();

        private int _counter;

        private GameWorld worldData;

        public EntityManager(GameWorld t)
        {
            worldData = t;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _entitiesUpdate.Count; i++)
            {
                var locArr = _entitiesUpdate[i];
                if (locArr is null)
                    continue;
                for (int c = 0; c < locArr.Length; c++)
                {
                    locArr[c].Update(gameTime);
                }
            }

            while(_toDelete.TryPop(out int r))
            {
                _entitiesUpdate[r] = null;
                _entitiesDraw[r] = null;
                _ids.Push(r);
            }
        }

        public void Draw(GameTime gameTime)
        {
            for (int i = 0; i < _entitiesDraw.Count; i++)
            {
                var locArr = _entitiesDraw[i];
                if (locArr is null)
                    continue;
                for (int c = 0; c < locArr.Length; c++)
                {
                    locArr[c].Update(gameTime);
                }
            }
        }

        public T GetOrThrow<T>(int id) where T : IUpdateComponent
        {
            if (TryGetComponent(id, out T comp))
                return comp;
            Throw(nameof(id));
            return default;
        }

        public EntityManager OutGetOrThrow<T>(int id, out T comp) where T : IUpdateComponent
        {
            if (TryGetComponent(id, out comp))
                return this;
            Throw(nameof(id));
            return this;
        }

        public bool TryGetComponent<T>(int id, [NotNullWhen(true)] out T comp) where T : IUpdateComponent
        {
            if(!IsValidID(id))
            {
                comp = default;
                return false;
            }
            IComponent[] arr = _entitiesUpdate[id];

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] is T comp1)
                {
                    comp = comp1;
                    return true;
                }
            }

            comp = default;
            return false;
        }

        private IComponent[] cache = new IComponent[64];

        public int AddEntity(IComponent a, IComponent b)
        {
            cache[0] = a;
            cache[1] = b;
            return AddEntity(2);
        }

        public int AddEntity(IComponent a)
        {
            cache[0] = a;
            return AddEntity(1);
        }

        public int AddEntity(IComponent a, IComponent b, IComponent c)
        {
            cache[0] = a;
            cache[1] = b;
            cache[2] = c;
            return AddEntity(3);
        }

        public int AddEntity(IComponent a, IComponent b, IComponent c, IComponent d)
        {
            cache[0] = a;
            cache[1] = b;
            cache[2] = c;
            cache[3] = d;
            return AddEntity(4);
        }


        private int AddEntity(int len)
        {
            int dE = 0;
            for(int i = 0; i < len; i++)
            {
                if (cache[i] is IDrawComponent)
                    dE++;
            }
            IComponent[] t = new IComponent[len - dE];
            IComponent[] d = new IComponent[dE];
            int ti = 0;
            int di = 0;
            for(int i = 0; i < len; i++)
            {
                if (cache[i] is not IDrawComponent)
                    t[ti++] = cache[i];
                else
                    d[di++] = cache[i];
            }

            int result;
            if(_ids.TryPop(out result))
            {
                _entitiesDraw[result] = d;
                _entitiesUpdate[result] = t;
            }
            else
            {
                _entitiesDraw.Add(d);
                _entitiesUpdate.Add(t);
                result = _counter++;
            }

            for(int i = 0; i < len; i++)
            {
                cache[i].Initalize(worldData, this, result);
            }

            return result;
        }

        public void RemoveEntity(int id)
        {
            if (!IsValidID(id))
                Throw(nameof(id));

            _toDelete.Push(id);
        }

        public bool IsValidID(int id)
        {
            return id >= 0 && id < _counter && _entitiesUpdate[id] != null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Throw(string name)
        {
            throw new ArgumentException(name);
        }
    }

}
