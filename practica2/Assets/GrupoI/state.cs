using UnityEngine.ProBuilder.Shapes;

public class State
{
    public int up;
    public int down;
    public int left;
    public int right;
    public int cercania;
    public int cuadrante;
    public State() { // Constructor
        up = 0; down = 0; left = 0; right = 0; cercania = 0; cuadrante = 0;
    }
    public State(int u, int d, int l, int r, int closer, int cuad)
    { // Constructor
        up = u; down = d; left = l; right = r; cercania = closer; cuadrante = cuad;
    }

    // Comparar dos clases estado
    public override bool Equals(object obj)
    {
        if (obj is State other)
        {
            return (this.up == other.up &&
                this.right == other.right &&
                this.down == other.down &&
                this.left == other.left &&
                this.cercania == other.cercania &&
                this.cuadrante == other.cuadrante);
        }
        return false;
    }
}
