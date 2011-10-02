﻿namespace Game.EventManagement.Events
{

    public class DamageEvent : Event
    {
        // Event Message Types
        public const string RECEIVE_DAMAGE = "actor_receive_damage";

        public int Damage { get; set; }
        public int SourceEntityID { get; set; }

        public DamageEvent(string type, int recipientID, int damage, int sourceEntityID)
            : base(type, recipientID)
        {
            this.Damage = damage > 0 ? damage : 0;  // Only positive amounts are valid
            this.SourceEntityID = sourceEntityID;
        }

        public override string ToString()
        {
            return base.ToString() + " [Damage: " + Damage + ", SourceEntityID: " + SourceEntityID + "]";
        }
    }

}
