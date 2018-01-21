namespace Engine
{
    public interface IGameComponent
    {
        void Init();
        void Update(float deltaTime);
        void Draw(float deltaTime);
        void Dispose();
    }
}
