using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.Graphics.Drawing;
using NuciXNA.Input;
using NuciXNA.Primitives;
using WalnutDrop.Model;
using ModelPlayerIndex = WalnutDrop.Model.PlayerIndex;

namespace WalnutDrop.Screens
{
    internal sealed class GameplayScreen
    {
        // Board layout constants
        private const int BoardX = 360;
        private const int BoardY = 80;
        private const int CellWidth = 70;
        private const int CellHeight = 75;
        private const int DropZoneHeight = 50;

        // Colours
        private static readonly Color ColourBackground    = new Color(15,  15,  35);
        private static readonly Color ColourBoardBg       = new Color(25,  25,  55);
        private static readonly Color ColourBoardBorder   = new Color(60,  60,  120);
        private static readonly Color ColourDropZoneIdle  = new Color(40,  40,  80);
        private static readonly Color ColourDropZoneHover = new Color(80,  100, 160);
        private static readonly Color ColourSwitchPad     = new Color(200, 170, 50);
        private static readonly Color ColourSwitchLever   = new Color(80,  80,  80);
        private static readonly Color ColourSwitchPivot   = new Color(220, 220, 220);
        private static readonly Color ColourCoin          = new Color(230, 190, 40);
        private static readonly Color ColourScoreSlotBg   = new Color(30,  30,  70);
        private static readonly Color ColourPlayer1       = new Color(80,  140, 220);
        private static readonly Color ColourPlayer2       = new Color(220, 80,  80);
        private static readonly Color ColourTextLight     = new Color(230, 230, 230);
        private static readonly Color ColourTextDim       = new Color(130, 130, 160);
        private static readonly Color ColourActiveTurn    = new Color(50,  200, 80);
        private static readonly Color ColourRoundComplete = new Color(220, 180, 50);
        private static readonly Color ColourGameOver      = new Color(220, 80,  50);

        private readonly GameState _gameState;
        private Texture2D _pixel;
        private SpriteFont _font;

        private TextSprite _roundLabel;
        private TextSprite _timerLabel;
        private TextSprite _p1NameLabel;
        private TextSprite _p1RoundScoreLabel;
        private TextSprite _p1TotalScoreLabel;
        private TextSprite _p1TurnLabel;
        private TextSprite _p2NameLabel;
        private TextSprite _p2RoundScoreLabel;
        private TextSprite _p2TotalScoreLabel;
        private TextSprite _p2TurnLabel;
        private TextSprite _statusLabel;

        private int _hoveredColumn;
        private Point2D _mousePosition;
        private List<CoinDropResult> _lastDropResults;

        public GameplayScreen()
        {
            _gameState = new GameState();
            _hoveredColumn = -1;
            _mousePosition = new Point2D(0, 0);
            _lastDropResults = new List<CoinDropResult>();
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new Color[] { Color.White });

            _font = NuciXNA.DataAccess.Content.NuciContentManager.Instance.LoadSpriteFont("Fonts/Default");

            _roundLabel = CreateTextSprite(new Point2D(440, 15), new Size2D(400, 40), Alignment.Middle, ColourTextLight);
            _timerLabel = CreateTextSprite(new Point2D(840, 15), new Size2D(120, 40), Alignment.End, ColourTextLight);

            _p1NameLabel       = CreateTextSprite(new Point2D(20,  160), new Size2D(320, 36), Alignment.Middle, ColourPlayer1);
            _p1RoundScoreLabel = CreateTextSprite(new Point2D(20,  210), new Size2D(320, 30), Alignment.Middle, ColourTextLight);
            _p1TotalScoreLabel = CreateTextSprite(new Point2D(20,  250), new Size2D(320, 26), Alignment.Middle, ColourTextDim);
            _p1TurnLabel       = CreateTextSprite(new Point2D(20,  300), new Size2D(320, 30), Alignment.Middle, ColourActiveTurn);

            _p2NameLabel       = CreateTextSprite(new Point2D(940, 160), new Size2D(320, 36), Alignment.Middle, ColourPlayer2);
            _p2RoundScoreLabel = CreateTextSprite(new Point2D(940, 210), new Size2D(320, 30), Alignment.Middle, ColourTextLight);
            _p2TotalScoreLabel = CreateTextSprite(new Point2D(940, 250), new Size2D(320, 26), Alignment.Middle, ColourTextDim);
            _p2TurnLabel       = CreateTextSprite(new Point2D(940, 300), new Size2D(320, 30), Alignment.Middle, ColourActiveTurn);

            _statusLabel = CreateTextSprite(new Point2D(360, 660), new Size2D(560, 40), Alignment.Middle, ColourRoundComplete);

            InputManager.Instance.MouseButtonPressed += OnMouseButtonPressed;
            InputManager.Instance.MouseMoved += OnMouseMoved;

            UpdateLabels();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameState.Update(deltaTime);

            _hoveredColumn = GetColumnAtPoint(_mousePosition);

            UpdateLabels();

            _roundLabel.Update(gameTime);
            _timerLabel.Update(gameTime);
            _p1NameLabel.Update(gameTime);
            _p1RoundScoreLabel.Update(gameTime);
            _p1TotalScoreLabel.Update(gameTime);
            _p1TurnLabel.Update(gameTime);
            _p2NameLabel.Update(gameTime);
            _p2RoundScoreLabel.Update(gameTime);
            _p2TotalScoreLabel.Update(gameTime);
            _p2TurnLabel.Update(gameTime);
            _statusLabel.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBoard(spriteBatch);
            DrawHud(spriteBatch);
        }

        private TextSprite CreateTextSprite(Point2D location, Size2D size, Alignment hAlign, Color tint)
        {
            Colour nuciTint = new Colour(tint.R, tint.G, tint.B, tint.A);

            TextSprite sprite = new TextSprite
            {
                FontName = "Default",
                Text = string.Empty,
                Location = location,
                SpriteSize = size,
                HorizontalAlignment = hAlign,
                VerticalAlignment = Alignment.Middle,
                Tint = nuciTint,
                Opacity = 1f
            };

            sprite.LoadContent();
            return sprite;
        }

        private void UpdateLabels()
        {
            _roundLabel.Text = "Round " + _gameState.CurrentRound.RoundNumber + " of " + RoundDefinition.All.Count;
            _timerLabel.Text = ((int)Math.Ceiling(_gameState.TurnTimeRemaining)).ToString() + "s";

            _p1NameLabel.Text = "PLAYER 1";
            _p1RoundScoreLabel.Text = "Score: " + _gameState.Player1.RoundScore + " / " + _gameState.CurrentRound.TargetScore;
            _p1TotalScoreLabel.Text = "Total: " + _gameState.Player1.TotalScore;

            _p2NameLabel.Text = "PLAYER 2";
            _p2RoundScoreLabel.Text = "Score: " + _gameState.Player2.RoundScore + " / " + _gameState.CurrentRound.TargetScore;
            _p2TotalScoreLabel.Text = "Total: " + _gameState.Player2.TotalScore;

            if (_gameState.Phase.Equals(GamePhase.Playing))
            {
                if (_gameState.CurrentPlayerIndex.Equals(ModelPlayerIndex.Player1))
                {
                    _p1TurnLabel.Text = "YOUR TURN";
                    _p2TurnLabel.Text = string.Empty;
                }
                else
                {
                    _p1TurnLabel.Text = string.Empty;
                    _p2TurnLabel.Text = "YOUR TURN";
                }

                _statusLabel.Text = string.Empty;

                if (_gameState.IsOpponentFinalDrop)
                {
                    _statusLabel.Text = "FINAL DROP!";
                }
            }
            else if (_gameState.Phase.Equals(GamePhase.RoundComplete))
            {
                _p1TurnLabel.Text = string.Empty;
                _p2TurnLabel.Text = string.Empty;
                _statusLabel.Text = "ROUND COMPLETE — NEXT ROUND STARTING...";
            }
            else
            {
                _p1TurnLabel.Text = string.Empty;
                _p2TurnLabel.Text = string.Empty;

                if (_gameState.Winner == null)
                {
                    _statusLabel.Text = "GAME OVER — IT'S A TIE!";
                }
                else if (_gameState.Winner.Equals(ModelPlayerIndex.Player1))
                {
                    _statusLabel.Text = "GAME OVER — PLAYER 1 WINS!";
                }
                else
                {
                    _statusLabel.Text = "GAME OVER — PLAYER 2 WINS!";
                }
            }

            if (_gameState.Phase.Equals(GamePhase.Playing))
            {
                SetTimerColour();
            }
        }

        private void SetTimerColour()
        {
            if (_gameState.TurnTimeRemaining <= 5f)
            {
                _timerLabel.Tint = new Colour(220, 60, 60, 255);
            }
            else if (_gameState.TurnTimeRemaining <= 10f)
            {
                _timerLabel.Tint = new Colour(220, 170, 50, 255);
            }
            else
            {
                _timerLabel.Tint = new Colour(230, 230, 230, 255);
            }
        }

        // ─── Board rendering ────────────────────────────────────────────────

        private void DrawBoard(SpriteBatch sb)
        {
            int boardWidth  = Board.Columns * CellWidth;
            int boardHeight = DropZoneHeight + Board.SwitchRowCount * CellHeight + CellHeight;

            // Board background
            DrawRect(sb, BoardX - 4, BoardY - 4, boardWidth + 8, boardHeight + 8, ColourBoardBorder);
            DrawRect(sb, BoardX, BoardY, boardWidth, boardHeight, ColourBoardBg);

            DrawDropZone(sb);
            DrawSwitches(sb);
            DrawScoringRow(sb);
        }

        private void DrawDropZone(SpriteBatch sb)
        {
            bool isPlayerTurn = _gameState.Phase.Equals(GamePhase.Playing);

            for (int col = 0; col < Board.Columns; col++)
            {
                int x = BoardX + col * CellWidth;
                int y = BoardY;

                Color fill;

                if (isPlayerTurn && _hoveredColumn.Equals(col))
                {
                    fill = ColourDropZoneHover;
                }
                else
                {
                    fill = ColourDropZoneIdle;
                }

                DrawRect(sb, x + 2, y + 2, CellWidth - 4, DropZoneHeight - 4, fill);

                if (isPlayerTurn && _hoveredColumn.Equals(col))
                {
                    // Draw a small downward arrow indicator
                    int arrowX = x + CellWidth / 2 - 4;
                    int arrowY = y + DropZoneHeight / 2 - 4;
                    DrawRect(sb, arrowX, arrowY, 8, 8, ColourCoin);
                }
            }
        }

        private void DrawSwitches(SpriteBatch sb)
        {
            for (int row = 0; row < Board.SwitchRowCount; row++)
            {
                int rowY = BoardY + DropZoneHeight + row * CellHeight;
                int visited = -1;

                for (int col = 0; col < Board.Columns; col++)
                {
                    GameSwitch sw = _gameState.Board.GetSwitch(row, col);

                    if (sw == null || sw.LeftColumn.Equals(visited))
                    {
                        continue;
                    }

                    visited = sw.LeftColumn;
                    DrawSwitch(sb, sw, rowY);
                }
            }
        }

        private void DrawSwitch(SpriteBatch sb, GameSwitch sw, int rowY)
        {
            int switchCenterX = BoardX + sw.LeftColumn * CellWidth + CellWidth;
            int switchMidY = rowY + CellHeight / 2;

            int halfLen = CellWidth / 2 - 6;
            int armH = 8;
            int tilt = 6;

            int leftY;
            int rightY;
            Color leftColor;
            Color rightColor;

            if (sw.PadSide.Equals(SwitchSide.Left))
            {
                leftY = switchMidY + tilt;
                rightY = switchMidY - tilt;
                leftColor = ColourSwitchPad;
                rightColor = ColourSwitchLever;
            }
            else
            {
                leftY = switchMidY - tilt;
                rightY = switchMidY + tilt;
                leftColor = ColourSwitchLever;
                rightColor = ColourSwitchPad;
            }

            // Left arm
            DrawRect(sb, switchCenterX - halfLen - CellWidth / 2 + 6, leftY - armH / 2, halfLen, armH, leftColor);
            // Right arm
            DrawRect(sb, switchCenterX + 2, rightY - armH / 2, halfLen, armH, rightColor);
            // Pivot dot
            DrawRect(sb, switchCenterX - 3, switchMidY - 3, 6, 6, ColourSwitchPivot);

            // Coin resting on pad
            if (sw.PadCoinCount > 0)
            {
                int padX;
                int padY;

                if (sw.PadSide.Equals(SwitchSide.Left))
                {
                    padX = switchCenterX - halfLen / 2 - CellWidth / 4;
                    padY = leftY;
                }
                else
                {
                    padX = switchCenterX + halfLen / 2 + CellWidth / 4;
                    padY = rightY;
                }

                DrawCoinAt(sb, padX - 10, padY - 22, sw.PadCoinCount);
            }
        }

        private void DrawScoringRow(SpriteBatch sb)
        {
            int rowY = BoardY + DropZoneHeight + Board.SwitchRowCount * CellHeight;

            for (int col = 0; col < Board.Columns; col++)
            {
                int x = BoardX + col * CellWidth;
                int value = _gameState.CurrentRound.SlotValues[col];

                DrawRect(sb, x + 2, rowY + 2, CellWidth - 4, CellHeight - 4, ColourScoreSlotBg);
                DrawRect(sb, x + 2, rowY + 2, CellWidth - 4, 3, ColourBoardBorder);

                string valueText = value.ToString();
                Vector2 textSize = _font.MeasureString(valueText);
                float textX = x + (CellWidth - textSize.X) / 2f;
                float textY = rowY + (CellHeight - textSize.Y) / 2f;
                sb.DrawString(_font, valueText, new Vector2(textX, textY), ColourTextLight);
            }
        }

        private void DrawCoinAt(SpriteBatch sb, int x, int y, int coinCount)
        {
            int size = 20;
            DrawRect(sb, x, y, size, size, ColourCoin);

            if (coinCount > 1)
            {
                string countText = coinCount.ToString();
                Vector2 textSize = _font.MeasureString(countText);
                float tx = x + (size - textSize.X) / 2f;
                float ty = y + (size - textSize.Y) / 2f;
                sb.DrawString(_font, countText, new Vector2(tx, ty), Color.Black);
            }
        }

        // ─── HUD rendering ───────────────────────────────────────────────────

        private void DrawHud(SpriteBatch sb)
        {
            // Top bar background
            DrawRect(sb, 0, 0, 1280, 70, new Color(10, 10, 28));

            // Player panels
            DrawPlayerPanel(sb, _gameState.Player1, 0,    360,  ColourPlayer1);
            DrawPlayerPanel(sb, _gameState.Player2, 920,  1280, ColourPlayer2);

            DrawProgressBar(sb, _gameState.Player1, 20,  380, 320, ColourPlayer1);
            DrawProgressBar(sb, _gameState.Player2, 940, 380, 320, ColourPlayer2);

            // Render text labels
            _roundLabel.Draw(sb);
            _timerLabel.Draw(sb);
            _p1NameLabel.Draw(sb);
            _p1RoundScoreLabel.Draw(sb);
            _p1TotalScoreLabel.Draw(sb);
            _p1TurnLabel.Draw(sb);
            _p2NameLabel.Draw(sb);
            _p2RoundScoreLabel.Draw(sb);
            _p2TotalScoreLabel.Draw(sb);
            _p2TurnLabel.Draw(sb);
            _statusLabel.Draw(sb);
        }

        private void DrawPlayerPanel(SpriteBatch sb, Player player, int panelLeft, int panelRight, Color accent)
        {
            int panelWidth = panelRight - panelLeft;
            DrawRect(sb, panelLeft, 70, panelWidth, 650, new Color(18, 18, 40));
            DrawRect(sb, panelRight - 2, 70, 2, 650, accent * 0.4f);
            DrawRect(sb, panelLeft, 70, 2, 650, accent * 0.4f);
        }

        private void DrawProgressBar(SpriteBatch sb, Player player, int x, int y, int width, Color accent)
        {
            int height = 16;
            int target = _gameState.CurrentRound.TargetScore;
            int score = player.RoundScore;

            DrawRect(sb, x, y, width, height, new Color(30, 30, 60));

            if (target > 0 && score > 0)
            {
                float fraction = Math.Min(1f, (float)score / target);
                int filledWidth = (int)(width * fraction);
                DrawRect(sb, x, y, filledWidth, height, accent * 0.8f);
            }

            DrawRect(sb, x, y, width, 2, accent * 0.5f);
            DrawRect(sb, x, y + height - 2, width, 2, accent * 0.5f);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private void DrawRect(SpriteBatch sb, int x, int y, int width, int height, Color color)
        {
            sb.Draw(_pixel, new Rectangle(x, y, width, height), color);
        }

        private int GetColumnAtPoint(Point2D point)
        {
            int boardRight = BoardX + Board.Columns * CellWidth;
            int boardBottom = BoardY + DropZoneHeight + Board.SwitchRowCount * CellHeight;

            if (point.X < BoardX || point.X >= boardRight)
            {
                return -1;
            }

            if (point.Y < BoardY || point.Y >= boardBottom)
            {
                return -1;
            }

            return (point.X - BoardX) / CellWidth;
        }

        // ─── Input handlers ──────────────────────────────────────────────────

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (!e.Button.Equals(MouseButton.Left))
            {
                return;
            }

            if (!_gameState.Phase.Equals(GamePhase.Playing))
            {
                return;
            }

            int col = GetColumnAtPoint(e.Location);

            if (col < 0)
            {
                return;
            }

            _lastDropResults = _gameState.DropCoin(col);
        }

        private void OnMouseMoved(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;
        }
    }
}
