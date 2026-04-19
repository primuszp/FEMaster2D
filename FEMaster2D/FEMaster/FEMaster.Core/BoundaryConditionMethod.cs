namespace FEMaster.Core
{
    public enum BoundaryConditionMethod
    {
        // Multiplies constrained diagonal entries by a large number.
        // Simple, but inflates the condition number of K.
        Penalty,

        // Removes constrained DOFs from the system entirely before solving.
        // Numerically superior: preserves the condition number of K.
        DirectCondensation
    }
}
