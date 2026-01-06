using  UnityEngine.UI;

namespace LowoUN.Modules.UI {

    public class NonRenderingRaycastBlocker : MaskableGraphic {
        protected NonRenderingRaycastBlocker () {
            useLegacyMeshGeneration = false;
        }
        protected override void OnPopulateMesh (VertexHelper toFill) {
            toFill.Clear ();
        }
    }
}