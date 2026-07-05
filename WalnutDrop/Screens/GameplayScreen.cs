using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics;
using NuciXNA.Gui;
using NuciXNA.Gui.Controls;
using NuciXNA.Gui.Screens;
using NuciXNA.Input;
using NuciXNA.Primitives;
using WalnutDrop.Model;
using ModelPlayerIndex = WalnutDrop.Model.PlayerIndex;

namespace WalnutDrop.Screens
{
    internal sealed class GameplayScreen : Screen
    {
        // Board layout constants
        private const int BoardX = 360;
        private const int BoardY = 80;
        private const int CellWidth = 70;
        private const int CellHeight = 75;
        private const int DropZoneHeight = 50;

        // Colours
        private static readonly Color ColourBackground    = new(15,  15,  35);
        private static readonly Color ColourBoardBg       = new(25,  25,  55);
        private static readonly Color ColourBoardBorder   = new(60,  60,  120);
        private static readonly Color ColourDropZoneIdle  = new(40,  40,  80);
        private static readonly Color ColourDropZoneHover = new(80,  100, 160);
        private static readonly Color ColourSwitchPad     = new(200, 170, 50);
        private static readonly Color ColourSwitchLever   = new(80,  80,  80);
        private static readonly Color ColourSwitchPivot   = new(220, 220, 220);
        private static readonly Color ColourCoin          = new(230, 190, 40);
        private static readonly Color ColourScoreSlotBg   = new(30,  30,  70);
        private static readonly Color ColourPlayer1       = new(80,  140, 220);
        private static readonly Color ColourPlayer2       = new(220, 80,  80);
        private static readonly Color ColourTextLight     = new(230, 230, 230);
        private static readonly Color ColourTextDim       = new(130, 130, 160);
        private static readonly Color ColourActiveTurn    = new(50,  200, 80);
        private static readonly Color ColourRoundComplete = new(220, 180, 50);
        private static readonly Color ColourGameOver      = new(220, 80,  50);

        private const int WalnutDisplaySize = 40;

        private readonly GameState gameState;
        private Texture2D pixel;
        private GuiImage walnutSprite;
        private SpriteFont font;

        private TextSprite roundLabel;
        private TextSprite timerLabel;
        private TextSprite p1NameLabel;
        private TextSprite p1RoundScoreLabel;
        private TextSprite p1TotalScoreLabel;
        private TextSprite p1TurnLabel;
        private TextSprite p2NameLabel;
        private TextSprite p2RoundScoreLabel;
        private TextSprite p2TotalScoreLabel;
        private TextSprite p2TurnLabel;
        private TextSprite statusLabel;

        private int hoveredColumn;
        private Point2D mousePosition;
        private IEnumerable<CoinDropResult> lastDropResults;
        private readonly List<FallingWalnut> fallingWalnuts;

        public GameplayScreen()
        {
            gameState = new GameState();
            hoveredColumn = -1;
            mousePosition = new Point2D(0, 0);
            lastDropResults = [];
            fallingWalnuts = [];

            BackgroundColour = new Colour(15, 15, 35);
        }

        protected override void DoLoadContent()
        {
            pixel = new Texture2D(GraphicsManager.Instance.Graphics.GraphicsDevice, 1, 1);
            pixel.SetData([Color.White]);

            walnutSprite = new GuiImage
            {
                ContentFile = "board/walnut",
                Size = new Size2D(WalnutDisplaySize, WalnutDisplaySize)
            };

            GuiManager.Instance.RegisterControls(walnutSprite);

            font = NuciXNA.DataAccess.Content.NuciContentManager.Instance.LoadSpriteFont("Fonts/Default");

            roundLabel = CreateTextSprite(new Point2D(440, 15), new Size2D(400, 40), Alignment.Middle, ColourTextLight);
            timerLabel = CreateTextSprite(new Point2D(840, 15), new Size2D(120, 40), Alignment.End, ColourTextLight);

            p1NameLabel       = CreateTextSprite(new Point2D(20,  160), new Size2D(320, 36), Alignment.Middle, ColourPlayer1);
            p1RoundScoreLabel = CreateTextSprite(new Point2D(20,  210), new Size2D(320, 30), Alignment.Middle, ColourTextLight);
            p1TotalScoreLabel = CreateTextSprite(new Point2D(20,  250), new Size2D(320, 26), Alignment.Middle, ColourTextDim);
            p1TurnLabel       = CreateTextSprite(new Point2D(20,  300), new Size2D(320, 30), Alignment.Middle, ColourActiveTurn);

            p2NameLabel       = CreateTextSprite(new Point2D(940, 160), new Size2D(320, 36), Alignment.Middle, ColourPlayer2);
            p2RoundScoreLabel = CreateTextSprite(new Point2D(940, 210), new Size2D(320, 30), Alignment.Middle, ColourTextLight);
            p2TotalScoreLabel = CreateTextSprite(new Point2D(940, 250), new Size2D(320, 26), Alignment.Middle, ColourTextDim);
            p2TurnLabel       = CreateTextSprite(new Point2D(940, 300), new Size2D(320, 30), Alignment.Middle, ColourActiveTurn);

            statusLabel = CreateTextSprite(new Point2D(360, 660), new Size2D(560, 40), Alignment.Middle, ColourRoundComplete);

            InputManager.Instance.MouseButtonPressed += OnMouseButtonPressed;
            InputManager.Instance.MouseMoved += OnMouseMoved;
            gameState.CoinDropped += OnCoinDropped;

            UpdateLabels();
        }

        protected override void DoUnloadContent()
        {
            InputManager.Instance.MouseButtonPressed -= OnMouseButtonPressed;
            InputManager.Instance.MouseMoved -= OnMouseMoved;
            gameState.CoinDropped -= OnCoinDropped;

            foreach (FallingWalnut walnut in fallingWalnuts)
            {
                walnut.Unload();
            }

            fallingWalnuts.Clear();
        }

        protected override void DoUpdate(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            gameState.Update(deltaTime);

            foreach (FallingWalnut walnut in fallingWalnuts)
            {
                walnut.Update(gameTime);
            }

            for (int i = fallingWalnuts.Count - 1; i >= 0; i--)
            {
                if (fallingWalnuts[i].IsComplete)
                {
                    fallingWalnuts[i].Unload();
                    fallingWalnuts.RemoveAt(i);
                }
            }

            hoveredColumn = GetColumnAtPoint(mousePosition);

            UpdateLabels();

            roundLabel.Update(gameTime);
            timerLabel.Update(gameTime);
            p1NameLabel.Update(gameTime);
            p1RoundScoreLabel.Update(gameTime);
            p1TotalScoreLabel.Update(gameTime);
            p1TurnLabel.Update(gameTime);
            p2NameLabel.Update(gameTime);
            p2RoundScoreLabel.Update(gameTime);
            p2TotalScoreLabel.Update(gameTime);
            p2TurnLabel.Update(gameTime);
            statusLabel.Update(gameTime);
        }

        protected override void DoDraw(SpriteBatch spriteBatch)
        {
            DrawBoard(spriteBatch);

            foreach (FallingWalnut walnut in fallingWalnuts)
            {
                walnut.Draw(spriteBatch);
            }

            DrawHud(spriteBatch);
        }

        private TextSprite CreateTextSprite(Point2D location, Size2D size, Alignment hAlign, Color tint)
        {
            Colour nuciTint = new(tint.R, tint.G, tint.B, tint.A);

            TextSprite sprite = new()
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
            roundLabel.Text = "Round " + gameState.CurrentRound.RoundNumber + " of " + RoundDefinition.All.Count;
            timerLabel.Text = ((int)Math.Ceiling(gameState.TurnTimeRemaining)).ToString() + "s";

            p1NameLabel.Text = "PLAYER 1";
            p1RoundScoreLabel.Text = "Score: " + gameState.Player1.RoundScore + " / " + gameState.CurrentRound.TargetScore;
            p1TotalScoreLabel.Text = "Total: " + gameState.Player1.TotalScore;

            p2NameLabel.Text = "PLAYER 2";
            p2RoundScoreLabel.Text = "Score: " + gameState.Player2.RoundScore + " / " + gameState.CurrentRound.TargetScore;
            p2TotalScoreLabel.Text = "Total: " + gameState.Player2.TotalScore;

            if (gameState.Phase.Equals(GamePhase.Playing))
            {
                if (gameState.CurrentPlayerIndex.Equals(ModelPlayerIndex.Player1))
                {
                    p1TurnLabel.Text = "YOUR TURN";
                    p2TurnLabel.Text = string.Empty;
                }
                else
                {
                    p1TurnLabel.Text = string.Empty;
                    p2TurnLabel.Text = "YOUR TURN";
                }

                statusLabel.Text = string.Empty;

                if (gameState.IsOpponentFinalDrop)
                {
                    statusLabel.Text = "FINAL DROP!";
                }
            }
            else if (gameState.Phase.Equals(GamePhase.RoundComplete))
            {
                p1TurnLabel.Text = string.Empty;
                p2TurnLabel.Text = string.Empty;
                statusLabel.Text = "ROUND COMPLETE — NEXT ROUND STARTING...";
            }
            else
            {
                p1TurnLabel.Text = string.Empty;
                p2TurnLabel.Text = string.Empty;

                if (gameState.Winner is null)
                {
                    statusLabel.Text = "GAME OVER — IT'S A TIE!";
                }
                else if (gameState.Winner.Equals(ModelPlayerIndex.Player1))
                {
                    statusLabel.Text = "GAME OVER — PLAYER 1 WINS!";
                }
                else
                {
                    statusLabel.Text = "GAME OVER — PLAYER 2 WINS!";
                }
            }

            if (gameState.Phase.Equals(GamePhase.Playing))
            {
                SetTimerColour();
            }
        }

        private void SetTimerColour()
        {
            if (gameState.TurnTimeRemaining <= 5f)
            {
                timerLabel.Tint = new Colour(220, 60, 60, 255);
            }
            else if (gameState.TurnTimeRemaining <= 10f)
            {
                timerLabel.Tint = new Colour(220, 170, 50, 255);
            }
            else
            {
                timerLabel.Tint = new Colour(230, 230, 230, 255);
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
            bool isPlayerTurn = gameState.Phase.Equals(GamePhase.Playing);

            for (int col = 0; col < Board.Columns; col++)
            {
                int x = BoardX + col * CellWidth;
                int y = BoardY;

                Color fill;

                if (isPlayerTurn && hoveredColumn.Equals(col))
                {
                    fill = ColourDropZoneHover;
                }
                else
                {
                    fill = ColourDropZoneIdle;
                }

                DrawRect(sb, x + 2, y + 2, CellWidth - 4, DropZoneHeight - 4, fill);

                if (isPlayerTurn && hoveredColumn.Equals(col))
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
                    GameSwitch sw = gameState.Board.GetSwitch(row, col);

                    if (sw is null || sw.LeftColumn.Equals(visited))
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
                int value = gameState.CurrentRound.SlotValues[col];

                DrawRect(sb, x + 2, rowY + 2, CellWidth - 4, CellHeight - 4, ColourScoreSlotBg);
                DrawRect(sb, x + 2, rowY + 2, CellWidth - 4, 3, ColourBoardBorder);

                string valueText = value.ToString();
                Vector2 textSize = font.MeasureString(valueText);
                float textX = x + (CellWidth - textSize.X) / 2f;
                float textY = rowY + (CellHeight - textSize.Y) / 2f;
                sb.DrawString(font, valueText, new Vector2(textX, textY), ColourTextLight);
            }
        }

        private void DrawCoinAt(SpriteBatch sb, int x, int y, int coinCount)
        {
            int drawX = x - WalnutDisplaySize / 2 + 10;
            int drawY = y - WalnutDisplaySize / 2 + 10;
            walnutSprite.Location = new Point2D(drawX, drawY);
            walnutSprite.Draw(sb);

            if (coinCount > 1)
            {
                string countText = coinCount.ToString();
                Vector2 textSize = font.MeasureString(countText);
                float tx = drawX + (WalnutDisplaySize - textSize.X) / 2f;
                float ty = drawY + (WalnutDisplaySize - textSize.Y) / 2f;
                sb.DrawString(font, countText, new Vector2(tx, ty), Color.Black);
            }
        }

        // ─── HUD rendering ───────────────────────────────────────────────────

        private void DrawHud(SpriteBatch sb)
        {
            // Top bar background
            DrawRect(sb, 0, 0, 1280, 70, new Color(10, 10, 28));

            // Player panels
            DrawPlayerPanel(sb, gameState.Player1, 0,    360,  ColourPlayer1);
            DrawPlayerPanel(sb, gameState.Player2, 920,  1280, ColourPlayer2);

            DrawProgressBar(sb, gameState.Player1, 20,  380, 320, ColourPlayer1);
            DrawProgressBar(sb, gameState.Player2, 940, 380, 320, ColourPlayer2);

            // Render text labels
            roundLabel.Draw(sb);
            timerLabel.Draw(sb);
            p1NameLabel.Draw(sb);
            p1RoundScoreLabel.Draw(sb);
            p1TotalScoreLabel.Draw(sb);
            p1TurnLabel.Draw(sb);
            p2NameLabel.Draw(sb);
            p2RoundScoreLabel.Draw(sb);
            p2TotalScoreLabel.Draw(sb);
            p2TurnLabel.Draw(sb);
            statusLabel.Draw(sb);
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
            int target = gameState.CurrentRound.TargetScore;
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
            sb.Draw(pixel, new Rectangle(x, y, width, height), color);
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

        private void OnCoinDropped(int column)
        {
            IEnumerable<(int col, int row)> logicalPath = gameState.Board.TracePrimaryPath(column);
            List<Point2D> screenWaypoints = new List<Point2D>();

            foreach ((int col, int row) in logicalPath)
            {
                screenWaypoints.Add(LogicalToScreen(col, row));
            }

            FallingWalnut walnut = new FallingWalnut(screenWaypoints, WalnutDisplaySize, 12f);
            fallingWalnuts.Add(walnut);
        }

        private Point2D LogicalToScreen(int column, int row)
        {
            int x = BoardX + column * CellWidth + (CellWidth - WalnutDisplaySize) / 2;
            int y;

            if (row < 0)
            {
                y = BoardY + (DropZoneHeight - WalnutDisplaySize) / 2;
            }
            else if (row >= Board.SwitchRowCount)
            {
                y = BoardY + DropZoneHeight + Board.SwitchRowCount * CellHeight + (CellHeight - WalnutDisplaySize) / 2;
            }
            else
            {
                y = BoardY + DropZoneHeight + row * CellHeight + (CellHeight - WalnutDisplaySize) / 2;
            }

            return new Point2D(x, y);
        }

        // ─── Input handlers ──────────────────────────────────────────────────

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (!e.Button.Equals(MouseButton.Left))
            {
                return;
            }

            if (!gameState.Phase.Equals(GamePhase.Playing))
            {
                return;
            }

            int col = GetColumnAtPoint(e.Location);

            if (col < 0)
            {
                return;
            }

            lastDropResults = gameState.DropCoin(col);
        }

        private void OnMouseMoved(object sender, MouseEventArgs e)
        {
            mousePosition = e.Location;
        }
    }
}
