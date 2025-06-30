using System;
using System.Collections.Generic;
using System.Text;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class BsodaaRoomFunction : RoomFunction
    {
        private SodaMachine[] sodaMachines;
        public BsodaaHelper Helper { get; private set; }

        public override void OnGenerationFinished()
        {
            base.OnGenerationFinished();

            Helper = room.objectObject.GetComponentInChildren<BsodaaHelper>();
            sodaMachines = room.objectObject.GetComponentsInChildren<SodaMachine>();
        }

        public bool HelperInStock => Helper == null || Helper.InStock;
        public bool MachinesInStock
        {
            get
            {
                foreach (SodaMachine machine in sodaMachines)
                {
                    if (machine.usesLeft > 0)
                        return true;
                }
                return false;
            }
        }
    }
}
