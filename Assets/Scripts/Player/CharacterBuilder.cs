using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// "Level Devil" tarzı küçük figürü kod ile parçalardan kurar:
    /// gövde (kafa+beden kapsülü), iki göz, salınan iki kol (eller dahil) ve
    /// yürüyüşte hareket eden iki bacak. Tüm parçalar yuvarlatılmış sprite'lardan.
    /// </summary>
    public static class CharacterBuilder
    {
        /// <summary>Visual kökünü oluşturur, animator'ı ekler ve döndürür.</summary>
        public static CharacterAnimator Build(Transform parent)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);
            // Hafif yukarı kaydır: ayaklar collider tabanıyla hizalansın, zemine gömülmesin.
            visual.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            // Gölge (zemine yapışık hafif leke)
            Part(visual.transform, "Shadow", new Vector3(0f, -0.52f, 0f),
                new Vector2(0.62f, 0.18f), new Color(0, 0, 0, 0.4f),
                7, SpriteFactory.SoftCircle);

            // Bacaklar (gövdenin arkasında, kalçadan salınır)
            var legL = Limb(visual.transform, "LegL", new Vector3(-0.13f, -0.34f, 0f),
                new Vector2(0.18f, 0.30f), SpriteFactory.CharLimb, 9, footSize: 0.20f);
            var legR = Limb(visual.transform, "LegR", new Vector3(0.13f, -0.34f, 0f),
                new Vector2(0.18f, 0.30f), SpriteFactory.CharLimb, 9, footSize: 0.20f);

            // Gövde anahattı + gövde (kafa ve beden tek kapsül)
            Part(visual.transform, "Outline", new Vector3(0f, 0.02f, 0f),
                new Vector2(0.72f, 1.0f), SpriteFactory.CharOutline, 8);
            Part(visual.transform, "Body", new Vector3(0f, 0.05f, 0f),
                new Vector2(0.62f, 0.9f), SpriteFactory.CharBody, 10);

            // Kollar (gövdenin önünde, omuzdan salınır, ucunda el)
            var armL = Limb(visual.transform, "ArmL", new Vector3(-0.30f, 0.22f, 0f),
                new Vector2(0.15f, 0.38f), SpriteFactory.CharLimb, 11, footSize: 0.17f);
            var armR = Limb(visual.transform, "ArmR", new Vector3(0.30f, 0.22f, 0f),
                new Vector2(0.15f, 0.38f), SpriteFactory.CharLimb, 11, footSize: 0.17f);

            // Gözler (öne bakan iki koyu oval)
            Part(visual.transform, "EyeL", new Vector3(-0.13f, 0.30f, 0f),
                new Vector2(0.13f, 0.17f), SpriteFactory.CharEye, 12);
            Part(visual.transform, "EyeR", new Vector3(0.13f, 0.30f, 0f),
                new Vector2(0.13f, 0.17f), SpriteFactory.CharEye, 12);
            // Göz parıltıları (küçük beyaz nokta)
            Part(visual.transform, "GlintL", new Vector3(-0.10f, 0.33f, 0f),
                new Vector2(0.05f, 0.05f), new Color(1, 1, 1, 0.9f), 13, SpriteFactory.Circle);
            Part(visual.transform, "GlintR", new Vector3(0.16f, 0.33f, 0f),
                new Vector2(0.05f, 0.05f), new Color(1, 1, 1, 0.9f), 13, SpriteFactory.Circle);

            var anim = visual.AddComponent<CharacterAnimator>();
            anim.legL = legL; anim.legR = legR;
            anim.armL = armL; anim.armR = armR;
            anim.body = visual.transform.Find("Body");
            return anim;
        }

        private static Transform Part(Transform parent, string name, Vector3 localPos,
            Vector2 size, Color color, int order, Sprite sprite = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : SpriteFactory.RoundedSquare;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        /// <summary>Omuzdan/kalçadan salınan uzuv. Pivot döndürülünce uzuv sallanır.</summary>
        private static Transform Limb(Transform parent, string name, Vector3 jointLocalPos,
            Vector2 size, Color color, int order, float footSize)
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = jointLocalPos;

            // Uzuv, pivotun altına sarkar.
            var seg = new GameObject("Seg");
            seg.transform.SetParent(pivot.transform, false);
            seg.transform.localPosition = new Vector3(0f, -size.y * 0.5f, 0f);
            seg.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.RoundedSquare;
            sr.color = color;
            sr.sortingOrder = order;

            // El / ayak ucu
            var foot = new GameObject("Tip");
            foot.transform.SetParent(pivot.transform, false);
            foot.transform.localPosition = new Vector3(0f, -size.y, 0f);
            foot.transform.localScale = new Vector3(footSize, footSize, 1f);
            var fsr = foot.AddComponent<SpriteRenderer>();
            fsr.sprite = SpriteFactory.Circle;
            fsr.color = color;
            fsr.sortingOrder = order + 1;

            return pivot.transform;
        }
    }
}
