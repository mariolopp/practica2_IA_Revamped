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
}
