namespace LevelEditor
{
	public class LevelObjectProperties
	{
		public bool GenerateSnapFaces { get; private set; }

		public bool CanOverlap { get; private set; }

		public bool HasEditorCollider { get; private set; }

		public LevelObjectProperties(LevelObjectProperties original)
		{
			GenerateSnapFaces = original.GenerateSnapFaces;
			CanOverlap = original.CanOverlap;
			HasEditorCollider = original.HasEditorCollider;
		}

		public LevelObjectProperties(bool snapFaces, bool overlap, bool EditorCollider)
		{
			GenerateSnapFaces = snapFaces;
			CanOverlap = overlap;
			HasEditorCollider = EditorCollider;
		}
	}
}
