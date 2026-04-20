using GuJian.Structures;

namespace GuJian.Tools {
    /// <summary>
    /// 工具属性修饰器：配件、镜头属性均实现。
    /// </summary>
    public interface IToolModifier {
        void ModifyDamage(StructureMaterial mat, ref float damage);
        void ModifyCritChance(ref float crit);
        void ModifyCritDamage(ref float critDmg);
        void ModifyCooldown(ref float cd);
        void ModifyRange(ref float range);
    }
}
