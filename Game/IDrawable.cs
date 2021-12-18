namespace Peanut.Libs.Game {
    /// <summary>
    /// Interface for drawable entities.
    /// </summary>
    public interface IDrawable {
        /// <summary>
        /// Called when this <see cref="IDrawable"/> should draw itself.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="Draw"/>.</param>
        void Draw(GameTime gameTime);
    }
}
