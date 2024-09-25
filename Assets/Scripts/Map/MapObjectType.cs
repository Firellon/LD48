namespace Map
{
    public enum MapObjectType
    {
        None = 0,
        Tree = 1,
        Grass = 2,
        Exit = 3,
        Bonfire = 4,
        Crate = 5,
        Corpse = 6,
        GuidePost = 7,
        Diary = 8,
        /// <summary>
        /// A decoy Map Object that eventually leads the Player to the Exit
        /// </summary>
        Waypoint = 9,
    }
}