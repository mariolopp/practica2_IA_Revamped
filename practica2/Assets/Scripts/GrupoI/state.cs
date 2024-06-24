using UnityEngine.ProBuilder.Shapes;

//--------------- CLASE PARA CREAR LOS ESTADOS ---------------------------
public class State
{
    #region Variables
    // Atributos de cada estado
    public int up;
    public int right;
    public int down;
    public int left;
    public int cercania;    //Discretizado
    public int cuadrante;
    #endregion

    #region Constructores
    public State() { 
        up = 0; right = 0; down = 0; left = 0; cercania = 0; cuadrante = 0;
    }
    public State(int u, int r, int d, int l, int closer, int cuad)
    {
        up = u; right = r; down = d; left = l;  cercania = closer; cuadrante = cuad;
    }
    #endregion

    #region Métodos
    // ------------------- MÉTODO COMPARAR OBJETOS STATE --------------------------
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

    //public bool isCorner()
    //{        
    //    State upRightCorner = new State(1, 1, 0, 0, 0, 4);
    //    State upLeftCorner = new State(1, 0, 0, 1, 0, 6);
    //    State downRightCorner = new State(0, 1, 1, 0, 0, 2);
    //    State downLeftCorner = new State(0, 0, 1, 1, 0, 0);

    //    if (Equals(upRightCorner) || Equals(upLeftCorner) || Equals(downRightCorner) || Equals(downLeftCorner))
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    #endregion
}
