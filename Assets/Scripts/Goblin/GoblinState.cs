public class GoblinState
{
    protected GoblinAI _goblinAI;

    // Constructor accepting GoblinAI
    public GoblinState(GoblinAI goblinAI)
    {
        _goblinAI = goblinAI;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
}