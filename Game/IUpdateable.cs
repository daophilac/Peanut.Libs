namespace Peanut.Libs.Game {
    /// <summary>
    /// Interface for updateable entities.
    /// </summary>
    public interface IUpdateable {
        /// <summary>
        /// Called when this <see cref="IUpdateable"/> should update itself.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="Update"/>.</param>
        void Update(GameTime gameTime);
    }
}
