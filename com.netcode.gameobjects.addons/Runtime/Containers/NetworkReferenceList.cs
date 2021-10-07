using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;
using UnityEngine;
using System.Collections;

namespace Unity.Netcode.Addons
{
    internal interface INetworkListNotify
    {
        void Notify_ElementAdded(int index);
        void Notify_ElementInserted(int index);
        void Notify_ElementRemovedAt(int index);
        void Notify_ElementAssigned(int index);
        void Notify_ElementMoved(int target, int source);    
    }


    [Serializable]
    public abstract class NetworkReferenceList<T> : 
        NetworkVariableBase,
        INetworkListNotify,
        IReadOnlyList<T>, 
        IReadOnlyCollection<T> 
        where T : struct, INetworkReference<T>
    {
        [SerializeField] 
        private List<T> m_List = new List<T>();
        private List<ReferenceListEvent<T>> m_DirtyEvents = new List<ReferenceListEvent<T>>();

        public delegate void OnListChangedDelegate(ReferenceListEvent<T> changeEvent);
        public event OnListChangedDelegate OnListChanged;
        
        public int Count
        {
            get
            {
                return m_List.Count;
            }
        }
        public T this[int index]
        {
            get => m_List[index];
            set
            {
                m_List[index] = value;
                (this as INetworkListNotify).Notify_ElementAssigned(index);
            }
        }

        public NetworkReferenceList() 
        {

        }
        public NetworkReferenceList(NetworkVariableReadPermission readPerm, IEnumerable<T> values) : base(readPerm)
        {
            foreach (var value in values)
            {
                m_List.Add(value);
            }
        }
        public NetworkReferenceList(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                m_List.Add(value);
            }
        }

        public void Add(T item)
        {
            m_List.Add(item);
            (this as INetworkListNotify).Notify_ElementAdded(m_List.Count - 1);
        }
        public void Clear()
        {
            m_List.Clear();

            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.Clear
            };

            HandleAddListEvent(listEvent);
        }
        public bool Contains(T item)
        {
            return m_List.Contains(item);
        }
        public bool Remove(T item)
        {
            var index = m_List.IndexOf(item);
            
            if (index == -1)
            {
                return false;
            }

            m_List.RemoveAt(index);
            (this as INetworkListNotify).Notify_ElementRemovedAt(index);
            return true;
        }
        public int IndexOf(T item)
        {
            return m_List.IndexOf(item);
        }
        public void Insert(int index, T item)
        {
            m_List.Insert(index, item);
            (this as INetworkListNotify).Notify_ElementInserted(index);
        }
        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
            (this as INetworkListNotify).Notify_ElementRemovedAt(index);
        }
        public void Move(int targetIndex, int sourceIndex)
        {
            if (targetIndex == sourceIndex)
                return;

            var element = m_List[sourceIndex];
            m_List.RemoveAt(sourceIndex);
            m_List.Insert(targetIndex, element);

            (this as INetworkListNotify).Notify_ElementMoved(targetIndex, sourceIndex);
        }

        public override void ResetDirty()
        {
            base.ResetDirty();
            m_DirtyEvents.Clear();
        }
        public override bool IsDirty()
        {
            // we call the base class to allow the SetDirty() mechanism to work
            return base.IsDirty() || m_DirtyEvents.Count > 0;
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            if (base.IsDirty())
            {
                writer.WriteValueSafe((ushort)1);
                writer.WriteValueSafe(ReferenceListEvent<T>.EventType.Full);
                WriteField(writer);

                return;
            }

            writer.WriteValueSafe((ushort)m_DirtyEvents.Count);
            for (int i = 0; i < m_DirtyEvents.Count; i++)
            {
                writer.WriteValueSafe(m_DirtyEvents[i].Type);
                switch (m_DirtyEvents[i].Type)
                {
                    case ReferenceListEvent<T>.EventType.Add:
                        m_DirtyEvents[i].Value.NetworkWrite(writer);
                        break;

                    case ReferenceListEvent<T>.EventType.Insert:
                    case ReferenceListEvent<T>.EventType.Value:
                        writer.WriteValueSafe(m_DirtyEvents[i].targetIndex);
                        m_DirtyEvents[i].Value.NetworkWrite(writer);
                        break;

                    case ReferenceListEvent<T>.EventType.RemoveAt:
                        writer.WriteValueSafe(m_DirtyEvents[i].targetIndex);
                        break;

                    case ReferenceListEvent<T>.EventType.Move:
                        writer.WriteValueSafe(m_DirtyEvents[i].targetIndex);
                        writer.WriteValueSafe(m_DirtyEvents[i].sourceIndex);
                        break;

                    case ReferenceListEvent<T>.EventType.Clear:
                        //Nothing has to be written
                        break;
                }
            }
        }
        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe((ushort)m_List.Count);
            for (int i = 0; i < m_List.Count; i++)
            {
                m_List[i].NetworkWrite(writer);
            }
        }
        
        public override void ReadField(FastBufferReader reader)
        {
            m_List.Clear();
            reader.ReadValueSafe(out ushort count);
            for (int i = 0; i < count; i++)
            {
                var value = default(T);
                value.NetworkRead(reader);
                m_List.Add(value);
            }
        }
        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            reader.ReadValueSafe(out ushort deltaCount);
            for (int i = 0; i < deltaCount; i++)
            {
                reader.ReadValueSafe(out ReferenceListEvent<T>.EventType eventType);
                switch (eventType)
                {
                    case ReferenceListEvent<T>.EventType.Add:
                        {
                            var value = default(T);
                            value.NetworkRead(reader);
                            m_List.Add(value);

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                    targetIndex = m_List.Count - 1,
                                    Value = m_List[m_List.Count - 1]
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType,
                                    targetIndex = m_List.Count - 1,
                                    Value = m_List[m_List.Count - 1]
                                });
                            }
                            break;
                        }
                    case ReferenceListEvent<T>.EventType.Insert:
                        {
                            var value = default(T);
                            reader.ReadValueSafe(out int index);
                            value.NetworkRead(reader);
                            m_List.Insert(index, value);

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = m_List[index]
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = m_List[index]
                                });
                            }

                            break;
                        }

                    case ReferenceListEvent<T>.EventType.RemoveAt:
                        {
                            reader.ReadValueSafe(out int index);
                            T value = m_List[index];
                            m_List.RemoveAt(index);

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = value
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = value
                                });
                            }
                        }
                        break;


                    case ReferenceListEvent<T>.EventType.Move:
                        {
                            reader.ReadValueSafe(out int targetIndex);
                            reader.ReadValueSafe(out int sourceIndex);
                            var value = m_List[sourceIndex];
                            m_List.RemoveAt(sourceIndex);
                            m_List.Insert(targetIndex, value);

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                    targetIndex = targetIndex,
                                    sourceIndex = sourceIndex,
                                    Value = value
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType,
                                    targetIndex = targetIndex,
                                    sourceIndex = sourceIndex,
                                    Value = value
                                });
                            }
                        }
                        break;

                    case ReferenceListEvent<T>.EventType.Value:
                        {
                            var value = default(T);
                            reader.ReadValueSafe(out int index);
                            value.NetworkRead(reader);
                            if (index < m_List.Count)
                            {
                                m_List[index] = value;
                            }

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = value
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType,
                                    targetIndex = index,
                                    Value = value
                                });
                            }
                        }
                        break;
                    case ReferenceListEvent<T>.EventType.Clear:
                        {
                            //Read nothing
                            m_List.Clear();

                            if (OnListChanged != null)
                            {
                                OnListChanged(new ReferenceListEvent<T>
                                {
                                    Type = eventType,
                                });
                            }

                            if (keepDirtyDelta)
                            {
                                m_DirtyEvents.Add(new ReferenceListEvent<T>()
                                {
                                    Type = eventType
                                });
                            }
                        }
                        break;
                    case ReferenceListEvent<T>.EventType.Full:
                        {
                            ReadField(reader);
                            ResetDirty();
                        }
                        break;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        void INetworkListNotify.Notify_ElementAdded(int index)
        {
            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.Add,
                Value = m_List[index],
                targetIndex = index
            };

            HandleAddListEvent(listEvent);
        }
        void INetworkListNotify.Notify_ElementInserted(int index)
        {
            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.Insert,
                targetIndex = index,
                Value = m_List[index]
            };

            HandleAddListEvent(listEvent);
        }
        void INetworkListNotify.Notify_ElementRemovedAt(int index)
        {
            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.RemoveAt,
                targetIndex = index
            };

            HandleAddListEvent(listEvent);
        }
        void INetworkListNotify.Notify_ElementAssigned(int index)
        {
            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.Value,
                targetIndex = index,
                Value = m_List[index]
            };

            HandleAddListEvent(listEvent);
        }
        void INetworkListNotify.Notify_ElementMoved(int target, int source)
        {
            var listEvent = new ReferenceListEvent<T>()
            {
                Type = ReferenceListEvent<T>.EventType.Move,
                targetIndex = target,
                sourceIndex = source
            };
            HandleAddListEvent(listEvent);
        }
        private void HandleAddListEvent(ReferenceListEvent<T> listEvent)
        {
            m_DirtyEvents.Add(listEvent);
            OnListChanged?.Invoke(listEvent);
        }
    }


    public struct ReferenceListEvent<T>
    {
        /// <summary>
        /// Enum representing the different operations available for triggering an event.
        /// </summary>
        public enum EventType : byte
        {
            Add,
            Insert,
            RemoveAt,
            Value,
            Move,
            Clear,
            Full
        }

        /// <summary>
        /// Enum representing the operation made to the list.
        /// </summary>
        public EventType Type;

        /// <summary>
        /// The value changed, added or removed (if available).
        /// </summary>
        public T Value;

        /// <summary>
        /// the index changed, added, removed or moved to (if available)
        /// </summary>
        public int targetIndex;
        /// <summary>
        /// the index that the element was moved from (if available)
        /// </summary>
        public int sourceIndex;
    }
}
