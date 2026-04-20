using GuJian.Pawns;

namespace GuJian.Tools {
    public interface ITool {
        ToolData Data { get; }
        IPawn Owner { get; }
        void OnEquip(IPawn owner);
        void OnUnequip();
        void Trigger(ToolActionType type, in ToolContext ctx);
        void Tick(float dt);
    }
}
