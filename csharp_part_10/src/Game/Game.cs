using Godot;
using System.Linq;

namespace SuperRogalik
{
    public partial class Game : Node2D
    {
        [Signal]
        public delegate void PlayerCreatedEventHandler(Entity player);

        Vector2I playerGridPos = Vector2I.Zero;
        Entity player;
        readonly EntityDefinition playerDefinition = ResourceLoader
            .Load<EntityDefinition>("res://assets/definitions/entities/actors/entity_definition_player.tres");

        InputHandler inputHandler;
        Map map;
        Camera2D camera;

        public override void _Ready()
        {
            inputHandler = GetNode<InputHandler>("InputHandler");
            map = GetNode<Map>("Map");
            camera = GetNode<Camera2D>("Camera2D");
        }

        public void NewGame()
        {
            player = new Entity(null, Vector2I.Zero, Entity.PLAYER);
            EmitSignal(SignalName.PlayerCreated, player);
            RemoveChild(camera);
            player.AddChild(camera);
            map.Generate(player);
            map.UpdateFov(player.GridPosition);
            MessageLog.SendMessage("Hello and welcome, adventurer, to yet another dungeon!", GameColors.WELCOME_TEXT);
            camera.CallDeferred(Camera2D.MethodName.MakeCurrent);
        }

        public bool LoadGame()
        {
            player = new Entity(null, Vector2I.Zero, "");
            RemoveChild(camera);
            player.AddChild(camera);
            if (!map.LoadGame(player))
                return false;
            EmitSignal(SignalName.PlayerCreated, player);
            map.UpdateFov(player.GridPosition);
            MessageLog.SendMessage("Welcome back, adventurer!", GameColors.WELCOME_TEXT);
            camera.CallDeferred(Camera2D.MethodName.MakeCurrent);
            return true;
        }

        public override async void _PhysicsProcess(double delta)
        {
            var action = await inputHandler.GetActionAsync(player);
            if (action != null && action.Perform())
            {
                HandleEnemyTurns();
                map.UpdateFov(player.GridPosition);
            }
        }

        private void HandleEnemyTurns()
        {
            foreach (var entity in GetMapData().GetActors().Where(a => a != player && a.IsAlive()))
            {
                entity.AIComponent.Perform();
            }
        }

        public MapData GetMapData() =>
            map.MapData;

    }
}
