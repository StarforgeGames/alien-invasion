﻿using System.Collections.Generic;
using Game.Processes;

namespace Game
{
    /// <summary>
    /// Simple Process Manager for continuing actions.
    /// </summary>
    public class ProcessManager
    {
        private List<Process> processes = new List<Process>();

        public void Attach(Process process)
        {
            processes.Add(process);
            process.IsAttached = true;
        }

        public void Detach(Process process)
        {
            processes.Remove(process);
            process.IsAttached = false;
        }

        public void Update(float deltaTime)
        {
            foreach (Process p in processes) {
                if (p.IsDead) {
                    if (p.Next != null) {
                        Attach(p.Next);
                        p.Next = null;
                    }

                    Detach(p);
                }
                else if(p.IsActive && !p.IsPaused) {
                    p.Update(deltaTime);
                }
            }
        }

        public void Reset()
        {
            processes.Clear();
        }
    }

}
