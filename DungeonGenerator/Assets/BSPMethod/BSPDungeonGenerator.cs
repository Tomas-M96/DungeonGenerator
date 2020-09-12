using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPDungeonGenerator : MonoBehaviour
{
    public int boardRows, boardColumns;
    public int minRoomSize, maxRoomSize;
    private List<Leaf> leafs = new List<Leaf>(); 
    private bool didSplit = true;

    public class Leaf
    {
        public Leaf leftChild, rightChild;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0); // i.e null
        public int debugId;

        private static int debugCounter = 0;

        public Leaf(Rect leafRect)
        {
            rect = leafRect;
            debugId = debugCounter;
            debugCounter++;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            if (leftChild != null || rightChild != null)
            {
                return false; //Leaf is already split, exit the function
            }

            // choose a vertical or horizontal split depending on the proportions
            // i.e. if too wide split vertically, or too long horizontally,
            // or if nearly square choose vertical or horizontal at random
            bool splitH = false;
            if (rect.width > rect.height && rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if (rect.height > rect.width && rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                Debug.Log("Sub-dungeon " + debugId + " will be a leaf");
                return false;
            }

            if (splitH)
            {
                // split so that the resulting sub-dungeons widths are not too small
                // (since we are splitting horizontally)
                int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));

                leftChild = new Leaf(new Rect(rect.x, rect.y, rect.width, split));
                rightChild = new Leaf(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));

                leftChild = new Leaf(new Rect(rect.x, rect.y, split, rect.height));
                rightChild = new Leaf(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }

            return true;
        }
    }

    public void CreateBSP(Leaf subDungeon)
    {
        Debug.Log("Splitting sub-dungeon " + subDungeon.debugId + ": " + subDungeon.rect);
        if (subDungeon.IAmLeaf())
        {
            // if the sub-dungeon is too large
            if (subDungeon.rect.width > maxRoomSize
              || subDungeon.rect.height > maxRoomSize
              || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (subDungeon.Split(minRoomSize, maxRoomSize))
                {
                    Debug.Log("Splitted sub-dungeon " + subDungeon.debugId + " in "
                      + subDungeon.left.debugId + ": " + subDungeon.left.rect + ", "
                      + subDungeon.right.debugId + ": " + subDungeon.right.rect);

                    CreateBSP(subDungeon.left);
                    CreateBSP(subDungeon.right);
                }
            }
        }
    }

    public void CreateRooms(Leaf leaf)
    {
        if (leaf.leftChild != null || leaf.rightChild != null)
        {
            if (leaf.leftChild != null)
            {
                leaf.leftChild.CreateRooms();
            }
            if (leaf.rightChild != null)
            {
                leaf.rightChild.CreateRooms();
            }
        }
    }


    void Start()
    {
        //Create the first room (i.e. board)
        Leaf rootLeaf = new Leaf(new Rect(0, 0, boardRows, boardColumns));
        CreateBSP(rootLeaf);
        leafs.Add(rootLeaf);

        while (didSplit)
        {
            didSplit = false;

            foreach (Leaf leaf in leafs)
            {
                if (leaf.leftChild == null && leaf.rightChild == null)
                {
                    if (leaf.rect.width > maxRoomSize || leaf.rect.height > maxRoomSize)
                    {
                        if (leaf.Split(minRoomSize, maxRoomSize))
                        {
                            leafs.Add(leaf.leftChild);
                            leafs.Add(leaf.rightChild);
                            didSplit = true;
                        }
                    }
                }
            }
        }
    }
}
