using System.Collections.Generic;
using System.Reflection;

namespace HSM {
    /// <summary>
    /// 状态机构建器类
    /// 用于构建和连接状态机的各个部分
    /// </summary>
    public class StateMachineBuilder {
        /// <summary>
        /// 状态机的根状态
        /// </summary>
        readonly State root;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="root">状态机的根状态</param>
        public StateMachineBuilder(State root) {
            this.root = root;
        }

        /// <summary>
        /// 构建状态机
        /// </summary>
        /// <returns>构建好的状态机实例</returns>
        public StateMachine Build() {
            var m = new StateMachine(root);
            Wire(root, m, new HashSet<State>());
            return m;
        }

        /// <summary>
        /// 连接状态机的各个部分
        /// </summary>
        /// <param name="s">当前要连接的状态</param>
        /// <param name="m">状态机实例</param>
        /// <param name="visited">已访问的状态集合，用于避免循环引用</param>
        void Wire(State s, StateMachine m, HashSet<State> visited) {
            if (s == null) return;
            if (!visited.Add(s)) return; // 状态已经连接过
            
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) machineField.SetValue(s, m);

            foreach (var fld in s.GetType().GetFields(flags)) {
                if (!typeof(State).IsAssignableFrom(fld.FieldType)) continue; // 只考虑State类型的字段
                if (fld.Name == "Parent") continue; // 跳过指向父状态的反向引用
                
                var child = (State)fld.GetValue(s);
                if (child == null) continue;
                if (!ReferenceEquals(child.Parent, s)) continue; // 确保是直接子状态
                
                Wire(child, m, visited); // 递归连接子状态
            }
        }
    }
}