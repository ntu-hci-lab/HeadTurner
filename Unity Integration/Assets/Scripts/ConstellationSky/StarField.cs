using System.Collections.Generic;
using UnityEngine;

public class StarField : MonoBehaviour {
    [Range(0, 100)]
    [SerializeField] private float starSizeMin = 0f;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;
    private List<StarDataLoader.Star> stars;
    private List<GameObject> starObjects;
    private Dictionary<int, GameObject> constellationVisible = new();

    private readonly int starFieldScale = 400;
    public bool Orion_0, Monceros_1, Gemini_2, Cancer_3, Leo_4, LeoMinor_5, Lynx_6, UrsaMajor_7, BigDipper_8,
                SmallBear_9, Swan_10, Cassiopeia_11, FlyHorse_12, Dragon_13, NorthHat_14, Hercules_15, Hound_16,
                Coma_17, Bootes_18;
    private int set_0 = 1, set_1 = 1, set_2 = 1, set_3 = 1, set_4 = 1, set_5 = 1, set_6 = 1, set_7 = 1, set_8 = 1,
                set_9 = 1, set_10 = 1, set_11 = 1, set_12 = 1, set_13 = 1, set_14 = 1, set_15 = 1, set_16 = 1, set_17 = 1, set_18 = 1;

    void Start()
    {
        // Read in the star data.
        StarDataLoader sdl = new();
        stars = sdl.LoadData();
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects.
            GameObject stargo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stargo.transform.parent = transform;
            stargo.name = $"HR {star.catalog_number}";
            stargo.transform.localPosition = star.position * starFieldScale;
            //stargo.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.size);
            stargo.transform.LookAt(transform.position);
            stargo.transform.Rotate(0, 180, 0);
            //stargo.AddComponent<MouseHover>();
            Material material = stargo.GetComponent<MeshRenderer>().material;
            material.shader = Shader.Find("Unlit/StarShader");
            material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, star.size));
            material.color = star.colour;
            starObjects.Add(stargo);
        }
    }

    // Could also do in Update with Time.deltatime scaling.
    //private void FixedUpdate()
    //{
    //    if (Input.GetKey(KeyCode.Mouse1))
    //    {
    //        Camera.main.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, Input.GetAxis("Mouse Y"));
    //        Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, -Input.GetAxis("Mouse X"));
    //    }
    //    return;
    //}

    private void OnValidate()
    {
        if (starObjects != null)
        {
            for (int i = 0; i < starObjects.Count; i++)
            {
                // Update the size set in the shader.
                Material material = starObjects[i].GetComponent<MeshRenderer>().material;
                material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, stars[i].size));
            }
        }
    }

    // A constellation is a tuple of the stars and the lines that join them.
    private readonly List<(int[], int[])> constellations = new()
    {
        // Orion_0
        (new int[] { 1948, 1903, 1852, 2004, 1713, 2061, 1790, 1907, 2124,
                 2199, 2135, 2047, 2159, 1543, 1544, 1570, 1552, 1567 },
       new int[] { 1713, 2004, 1713, 1852, 1852, 1790, 1852, 1903, 1903, 1948,
                 1948, 2061, 1948, 2004, 1790, 1907, 1907, 2061, 2061, 2124,
                 2124, 2199, 2199, 2135, 2199, 2159, 2159, 2047, 1790, 1543,
                 1543, 1544, 1544, 1570, 1543, 1552, 1552, 1567, 2135, 2047 }),
        // Monceros_1
        (new int[] { 2970, 3188, 2714, 2356, 2227, 2506, 2298, 2385, 2456, 2479 },
       new int[] { 2970, 3188, 3188, 2714, 2714, 2356, 2356, 2227, 2714, 2506,
                 2506, 2298, 2298, 2385, 2385, 2456, 2479, 2506, 2479, 2385 }),
        // Gemini_2
        (new int[] { 2890, 2891, 2990, 2421, 2777, 2473, 2650, 2216, 2895,
                 2343, 2484, 2286, 2134, 2763, 2697, 2540, 2821, 2905, 2985},
       new int[] { 2890, 2697, 2990, 2905, 2697, 2473, 2905, 2777, 2777, 2650,
                 2650, 2421, 2473, 2286, 2286, 2216, 2473, 2343, 2216, 2134,
                 2763, 2484, 2763, 2777, 2697, 2540, 2697, 2821, 2821, 2905, 2905, 2985 }),
        // Cancer_3
        (new int[] { 3475, 3449, 3461, 3572, 3249 },
       new int[] { 3475, 3449, 3449, 3461, 3461, 3572, 3461, 3249 }),
        // Leo_4
        (new int[] { 3982, 4534, 4057, 4357, 3873, 4031, 4359, 3975, 4399, 4386, 3905, 3773, 3731 },
       new int[] { 4534, 4357, 4534, 4359, 4357, 4359, 4357, 4057, 4057, 4031,
                 4057, 3975, 3975, 3982, 3975, 4359, 4359, 4399, 4399, 4386,
                 4031, 3905, 3905, 3873, 3873, 3975, 3873, 3773, 3773, 3731, 3731, 3905 }),
        // Leo Minor_5
        (new int[] { 3800, 3974, 4100, 4247, 4090 },
       new int[] { 3800, 3974, 3974, 4100, 4100, 4247, 4247, 4090, 4090, 3974 }),
        // Lynx_6
        (new int[] { 3705, 3690, 3612, 3579, 3275, 2818, 2560, 2238 },
       new int[] { 3705, 3690, 3690, 3612, 3612, 3579, 3579, 3275, 3275, 2818,
                 2818, 2560, 2560, 2238 }),
        // UrsaMajor_7
        (new int[] { 3569, 3594, 3775, 3888, 3323, 3757, 4301, 4295, 4554, 4660,
                 4905, 5054, 5191, 4518, 4335, 4069, 4033, 4377, 4375 },
       new int[] { 3569, 3594, 3594, 3775, 3775, 3888, 3888, 3323, 3323, 3757,
                 3757, 3888, 3757, 4301, 4301, 4295, 4295, 3888, 4295, 4554,
                 4554, 4660, 4660, 4301, 4660, 4905, 4905, 5054, 5054, 5191,
                 4554, 4518, 4518, 4335, 4335, 4069, 4069, 4033, 4518, 4377, 4377, 4375 }),
        //BigDipper_8
        (new int[] { 4301, 4295, 4554, 4660, 4905, 5062, 5191 },
        new int[]
        {
          4301,4295,4295,4554,4554,4660,4660,4905,4905,5062,5062,5191
        }),
        //SmallBear_9
        (new int[] { 424, 6789, 6322, 5903, 5563, 5714, 6116 },
        new int[]
        {
          424,6789,6789,6322,6322,5903,5903,5563,5563,5714,5714,6116,6116,5903
        }),
        //Swan_10
        (new int[] { 7924, 7796, 7949, 8115, 7528, 7328, 7615, 7417 },
        new int[]
        {
          7924,7796,7796,7949,7949,8115,7796,7528,7528,7328,7796,7615,7615,7417
        }),
        //Cassiopeia_11
        (new int[] { 542, 403, 264, 168, 21 },
        new int[]
        {
          542,403,403,264,264,168,168,21
        }),
        //FlyHorse_12
        (new int[] { 604, 337, 269, 165, 15, 39, 8775, 8781, 8650, 8454, 8665, 8634, 8450, 8308 },
        new int[]
        {
          604,337,337,269,337,165,165,15,15,39,15,8775,39,8781,8775,8650,8650,8454,
          8775,8781,8781,8665,8665,8634,8634,8450,8450,8308,8308,8634
        }),
        //Dragon_13
        (new int[] { 6705, 6536, 6688, 7310, 7582, 7352, 6920, 6396, 6132, 5986, 5744, 5291, 4787 },
        new int[]
        {
          6705,6688,6536,6688,6688,7310,7310,7582,7582,7352,7352,6920,6920,6396,6396,
          6132,6132,5986,5986,5744,5744,5291,5291,4787
        }),
        //NorthHat_14
        (new int[] { 5971, 5958, 5849, 5793, 5747, 5778 },
        new int[]
        {
          5971,5958,5958,5849,5849,5793,5793,5747,5747,5778
        }),
        //Hercules_15
        (new int[] { 6220, 6418, 6324, 6410, 6407, 6148, 6212 },
        new int[]
        {
          6220,6418,6418,6324,6324,6410,6410,6407,6407,6148,6148,6212,6212,6324
        }),
        //Hound_16
        (new int[] { 4914, 4785 },
        new int[]
        {
          4914,4785
        }),
        //Coma_17
        (new int[] { 4983, 4737 },
        new int[]
        {
          4983,4737
        }),
        //Bootes_18
        (new int[] { 5506, 5429, 5435, 5602, 5681, 5340, 5235, 5473 },
        new int[]
        {
          5506,5429,5429,5435,5435,5602,5602,5681,5681,5506,5506,5340,5340,5235,5340,5473
        }),
    };

    private void Update()
    {
        // Check for numeric presses and toggle the constellation highlighting.
        //for (int i = 0; i < 10; i++)
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha0 + i))
        //    {
        //        ToggleConstellation(i);
        //    }
        //}
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Camera.main.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, Input.GetAxis("Mouse Y"));
            Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, -Input.GetAxis("Mouse X"));
        }
        ///Orion_0
        if (Orion_0 && set_0 % 2 != 0)
        {
            CreateConstellation(0);
            set_0++;
        }
        else if (Orion_0 == false && set_0 % 2 == 0)
        {
            RemoveConstellation(0);
            set_0--;
        }
        ///Monceros_1
        if (Monceros_1 && set_1 % 2 != 0)
        {
            CreateConstellation(1);
            set_1++;
        }
        else if (Monceros_1 == false && set_1 % 2 == 0)
        {
            RemoveConstellation(1);
            set_1--;
        }
        ///Gemini_2
        if (Gemini_2 && set_2 % 2 != 0)
        {
            CreateConstellation(2);
            set_2++;
        }
        else if (Gemini_2 == false && set_2 % 2 == 0)
        {
            RemoveConstellation(2);
            set_2--;
        }
        ///Cancer_3
        if (Cancer_3 && set_3 % 2 != 0)
        {
            CreateConstellation(3);
            set_3++;
        }
        else if (Cancer_3 == false && set_3 % 2 == 0)
        {
            RemoveConstellation(3);
            set_3--;
        }
        ///Leo_4
        if (Leo_4 && set_4 % 2 != 0)
        {
            CreateConstellation(4);
            set_4++;
        }
        else if (Leo_4 == false && set_4 % 2 == 0)
        {
            RemoveConstellation(4);
            set_4--;
        }
        ///LeoMinor_5
        if (LeoMinor_5 && set_5 % 2 != 0)
        {
            CreateConstellation(5);
            set_5++;
        }
        else if (LeoMinor_5 == false && set_5 % 2 == 0)
        {
            RemoveConstellation(5);
            set_5--;
        }
        ///Lynx_6
        if (Lynx_6 && set_6 % 2 != 0)
        {
            CreateConstellation(6);
            set_6++;
        }
        else if (Lynx_6 == false && set_6 % 2 == 0)
        {
            RemoveConstellation(6);
            set_6--;
        }
        ///UrsaMajor_7
        if (UrsaMajor_7 && set_7 % 2 != 0)
        {
            CreateConstellation(7);
            set_7++;
        }
        else if (UrsaMajor_7 == false && set_7 % 2 == 0)
        {
            RemoveConstellation(7);
            set_7--;
        }
        ///BigDipper_8
        if (BigDipper_8 && set_8 % 2 != 0)
        {
            CreateConstellation(8);
            set_8++;
        }
        else if (BigDipper_8 == false && set_8 % 2 == 0)
        {
            RemoveConstellation(8);
            set_8--;
        }
        ///SmallBear_9, Swan_10
        if (SmallBear_9 && set_9 % 2 != 0)
        {
            CreateConstellation(9);
            set_9++;
        }
        else if (SmallBear_9 == false && set_9 % 2 == 0)
        {
            RemoveConstellation(9);
            set_9--;
        }
        ///Swan_10
        if (Swan_10 && set_10 % 2 != 0)
        {
            CreateConstellation(10);
            set_10++;
        }
        else if (Swan_10 == false && set_10 % 2 == 0)
        {
            RemoveConstellation(10);
            set_10--;
        }
        //Cassiopeia_11
        if (Cassiopeia_11 && set_11 % 2 != 0)
        {
            CreateConstellation(11);
            set_11++;
        }
        else if (Cassiopeia_11 == false && set_11 % 2 == 0)
        {
            RemoveConstellation(11);
            set_11--;
        }
        //FlyHorse_12
        if (FlyHorse_12 && set_12 % 2 != 0)
        {
            CreateConstellation(12);
            set_12++;
        }
        else if (FlyHorse_12 == false && set_12 % 2 == 0)
        {
            RemoveConstellation(12);
            set_12--;
        }
        //Dragon_13
        if (Dragon_13 && set_13 % 2 != 0)
        {
            CreateConstellation(13);
            set_13++;
        }
        else if (Dragon_13 == false && set_13 % 2 == 0)
        {
            RemoveConstellation(13);
            set_13--;
        }
        //NorthHat_14
        if (NorthHat_14 && set_14 % 2 != 0)
        {
            CreateConstellation(14);
            set_14++;
        }
        else if (NorthHat_14 == false && set_14 % 2 == 0)
        {
            RemoveConstellation(14);
            set_14--;
        }
        //Hercules_15
        if (Hercules_15 && set_15 % 2 != 0)
        {
            CreateConstellation(15);
            set_15++;
        }
        else if (Hercules_15 == false && set_15 % 2 == 0)
        {
            RemoveConstellation(15);
            set_15--;
        }
        //Hound_16
        if (Hound_16 && set_16 % 2 != 0)
        {
            CreateConstellation(16);
            set_16++;
        }
        else if (Hound_16 == false && set_16 % 2 == 0)
        {
            RemoveConstellation(16);
            set_16--;
        }
        //Coma_17
        if (Coma_17 && set_17 % 2 != 0)
        {
            CreateConstellation(17);
            set_17++;
        }
        else if (Coma_17 == false && set_17 % 2 == 0)
        {
            RemoveConstellation(17);
            set_17--;
        }
        //Bootes_18
        if (Bootes_18 && set_18 % 2 != 0)
        {
            CreateConstellation(18);
            set_18++;
        }
        else if (Bootes_18 == false && set_18 % 2 == 0)
        {
            RemoveConstellation(18);
            set_18--;
        }
        //Debug.Log(constellations.Count);
    }

    void ToggleConstellation(int index)
    {
        // Safety check the index is valid.
        if ((index < 0) || (index >= constellations.Count))
        {
            return;
        }
        // Toggle on or off.
        if (constellationVisible.ContainsKey(index))
        {
            RemoveConstellation(index);
        }
        else
        {
            CreateConstellation(index);
        }
    }

    void CreateConstellation(int index)
    {
        int[] constellation = constellations[index].Item1;
        int[] lines = constellations[index].Item2;

        // Change the colours of the stars
        foreach (int catalogNumber in constellation)
        {
            // Remember list is 0-up catalog numbers are 1-up.
            starObjects[catalogNumber - 1].GetComponent<MeshRenderer>().material.color = new Color(0,255,255);
        }

        GameObject constellationHolder = new($"Constellation {index}");
        constellationHolder.transform.parent = transform;
        constellationVisible[index] = constellationHolder;

        // Draw the constellation lines.
        for (int i = 0; i < lines.Length; i += 2)
        {
            // Parent it to our constellation object so we can delete them all later.
            GameObject line = new("Line");
            line.transform.parent = constellationHolder.transform;
            // Defaults to white and width 1 which works for us.
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            // Doesn't get assigned a material so just dig out one that works.
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            Color c1 = new Color(0, 255, 255);
            Color c2 = new Color(0, 255, 255);
            lineRenderer.SetColors( c1,c2);
            // Disable useWorldSpace so it will track the parent game object.
            lineRenderer.useWorldSpace = false;
            Vector3 pos1 = starObjects[lines[i] - 1].transform.position;
            Vector3 pos2 = starObjects[lines[i + 1] - 1].transform.position;
            // Offset them so they don't occlude the stars, 3 chosen by trial and error.
            Vector3 dir = (pos2 - pos1).normalized * 3;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, pos1 + dir);
            lineRenderer.SetPosition(1, pos2 - dir);
        }
    }

    void RemoveConstellation(int index)
    {
        int[] constallation = constellations[index].Item1;

        // Toggling off set the stars back to the original colour.
        foreach (int catalogNumber in constallation)
        {
            // Remember list is 0-up catalog numbers are 1-up.
            starObjects[catalogNumber - 1].GetComponent<MeshRenderer>().material.color = stars[catalogNumber - 1].colour;
        }
        // Remove the constellation lines.
        Destroy(constellationVisible[index]);
        // Remove from our dictionary as it's now off.
        constellationVisible.Remove(index);
    }

}

