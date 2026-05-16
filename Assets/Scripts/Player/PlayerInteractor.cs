using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraFocusController cameraFocusController;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float sphereCastRadius = 0.35f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Hint")]
    [SerializeField] private float hintHeight = 0.6f;

    [Header("Throw")]
    [SerializeField] private float throwForce = 3f;

    private HoldableObject _heldObject;
    private HoldableObject _hoveredHoldable;
    private NPCClient _hoveredNPC;
    private ItemSlot _hoveredSlot;
    private ItemSlot _lastHighlightedSlot;

    private StirMiniGame _activeStirMiniGame;

    private bool _wasCameraFocused;

    private void Start()
    {
        if (cameraFocusController == null)
            cameraFocusController = FindFirstObjectByType<CameraFocusController>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (uiManager != null)
        {
            uiManager.SetCrosshairVisible(true);
            uiManager.HideWorldHint();
            uiManager.HideFocusHint();
        }
    }

    private void Update()
    {
        if (CursorLockController.IsUIOpen)
        {
            ClearHover();
            ClearSlotHighlight();
            HideFocusHint();
            return;
        }

        if (cameraFocusController != null && cameraFocusController.IsFocused)
        {
            UpdateFocusMode();
            return;
        }

        HideFocusHint();

        if (_heldObject != null)
            UpdateSlotHover();
        else
            UpdateObjectHover();

        HandleInput();
        UpdateHeldObjectFocusPose();
    }

    private void UpdateFocusMode()
    {
        ClearHover();
        ClearSlotHighlight();

        ShowCurrentFocusHint();

        if (PressedExitFocusKey())
        {
            ExitFocusMode();
            return;
        }

        HandleFocusInput();
    }

    private void ShowCurrentFocusHint()
    {
        if (uiManager == null)
            return;

        if (_activeStirMiniGame != null && _activeStirMiniGame.IsActive)
        {
            uiManager.ShowFocusHint(
                "[КРУГИ МЫШЬЮ] мешать\n" +
                "[WASD/Space] выйти"
            );
            return;
        }

        if (_heldObject != null)
        {
            if (IsHoldingSpoon())
            {
                uiManager.ShowFocusHint(
                    "[КРУГИ МЫШЬЮ] мешать\n" +
                    "[E] бросить\n" +
                    "[WASD/Space] выйти"
                );
            }
            else if (IsHoldingPourableBottle())
            {
                uiManager.ShowFocusHint(
                    "[ПКМ] налить\n" +
                    "[E] бросить\n" +
                    "[WASD/Space] выйти"
                );
            }
            else
            {
                uiManager.ShowFocusHint(
                    "[E] бросить\n" +
                    "[WASD/Space] выйти"
                );
            }
        }
        else
        {
            uiManager.ShowFocusHint("[WASD/Space] выйти");
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPrimaryClickInteraction();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_heldObject != null)
            {
                if (_hoveredSlot != null)
                    PlaceHeldObjectInSlot();
                else
                    DropHeldObject();
            }
            else
            {
                TryPickUp();
            }
        }
    }

    private void HandleFocusInput()
    {
        if (_heldObject == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitFocusMode();
            DropHeldObject();
        }
    }

    private void TryPrimaryClickInteraction()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitSomething = Physics.SphereCast(
            ray,
            sphereCastRadius,
            out RaycastHit hit,
            interactDistance,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        if (!hitSomething)
            return;

        TrashBin trashBin = hit.collider.GetComponentInParent<TrashBin>();

        if (trashBin != null)
        {
            if (_heldObject != null && IsHoldingGlass())
            {
                trashBin.ClearGlass();

                if (uiManager != null)
                    uiManager.SetStatusText("Стакан очищен.");

                return;
            }

            if (uiManager != null)
                uiManager.SetStatusText("Возьмите стакан, чтобы очистить его.");

            return;
        }

        Shaker shaker = hit.collider.GetComponentInParent<Shaker>();

        if (shaker != null)
        {
            if (_heldObject != null && IsHoldingGlass())
            {
                shaker.StartShakeMode(_heldObject);
                return;
            }

            if (uiManager != null)
                uiManager.SetStatusText("Возьмите стакан, чтобы использовать шейкер.");

            return;
        }

        GlassFocusTarget glassFocusTarget =
            hit.collider.GetComponentInParent<GlassFocusTarget>();

        if (glassFocusTarget != null)
        {
            if (_heldObject != null)
            {
                Garnish garnish = _heldObject.GetComponent<Garnish>();

                if (garnish == null)
                    garnish = _heldObject.GetComponentInChildren<Garnish>();

                if (garnish != null)
                {
                    GlassItemReceiver itemReceiver =
                        hit.collider.GetComponentInParent<GlassItemReceiver>();

                    if (itemReceiver == null)
                        itemReceiver = glassFocusTarget.GetComponentInChildren<GlassItemReceiver>();

                    if (itemReceiver != null)
                    {
                        itemReceiver.AddGarnish(garnish);

                        _heldObject = null;

                        ClearHover();
                        ClearSlotHighlight();

                        return;
                    }

                    if (uiManager != null)
                        uiManager.SetStatusText("Не найден приёмник украшений на стакане.");

                    return;
                }
            }

            HandleGlassClick(glassFocusTarget);
            return;
        }

        NPCClient npc = hit.collider.GetComponentInParent<NPCClient>();

        if (npc != null)
        {
            if (_heldObject != null && IsHoldingGlass())
            {
                ServeDrinkToNPC();
                return;
            }

            npc.RequestOrderDialogue();
            return;
        }
    }

    private void HandleGlassClick(GlassFocusTarget glassFocusTarget)
    {
        if (glassFocusTarget == null)
            return;

        if (_heldObject == null)
        {
            glassFocusTarget.FocusCamera();
            return;
        }

        if (IsHoldingSpoon())
        {
            glassFocusTarget.StartStirMode(_heldObject);
            _activeStirMiniGame = glassFocusTarget.StirMiniGame;
        }
        else
        {
            glassFocusTarget.FocusCamera();
            _heldObject.EnterFocusPose();
        }

        _wasCameraFocused = true;
    }

    private void UpdateObjectHover()
    {
        ClearHover();
        ClearSlotHighlight();

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitSomething = Physics.SphereCast(
            ray,
            sphereCastRadius,
            out RaycastHit hit,
            interactDistance,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        if (!hitSomething)
            return;

        TrashBin trashBin = hit.collider.GetComponentInParent<TrashBin>();

        if (trashBin != null)
        {
            ShowHintAboveObject(
                "Возьмите стакан,\nчтобы очистить",
                trashBin.transform
            );

            return;
        }

        NPCClient npc = hit.collider.GetComponentInParent<NPCClient>();

        if (npc != null)
        {
            _hoveredNPC = npc;

            ShowHintAboveObject(
                "[ЛКМ]\nпринять заказ",
                npc.transform
            );

            return;
        }

        GlassFocusTarget glassFocusTarget =
            hit.collider.GetComponentInParent<GlassFocusTarget>();

        if (glassFocusTarget != null)
        {
            HoldableObject glassHoldable =
                hit.collider.GetComponentInParent<HoldableObject>();

            if (glassHoldable != null && !glassHoldable.IsHeld)
            {
                _hoveredHoldable = glassHoldable;

                ShowHintAboveObject(
                    "[E] взять стакан\n[ЛКМ] режим стакана",
                    glassFocusTarget.transform
                );

                return;
            }

            ShowHintAboveObject(
                "[ЛКМ]\nрежим стакана",
                glassFocusTarget.transform
            );

            return;
        }

        HoldableObject holdable =
            hit.collider.GetComponentInParent<HoldableObject>();

        if (holdable != null && !holdable.IsHeld)
        {
            _hoveredHoldable = holdable;

            ShowHintAboveObject(
                $"[E]\n{holdable.DisplayName}",
                holdable.transform
            );

            return;
        }
    }
    private void UpdateSlotHover()
    {
        _hoveredSlot = null;

        if (playerCamera == null || _heldObject == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitSomething = Physics.SphereCast(
            ray,
            sphereCastRadius,
            out RaycastHit hit,
            interactDistance,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        if (!hitSomething)
        {
            ClearSlotHighlight();
            ShowHeldObjectHint();
            return;
        }

        TrashBin trashBin = hit.collider.GetComponentInParent<TrashBin>();

        if (trashBin != null)
        {
            ClearSlotHighlight();

            if (IsHoldingGlass())
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nочистить стакан",
                    trashBin.transform
                );
            }
            else
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nмусорка",
                    trashBin.transform
                );
            }

            return;
        }

        NPCClient npc = hit.collider.GetComponentInParent<NPCClient>();

        if (npc != null)
        {
            ClearSlotHighlight();

            if (IsHoldingGlass())
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nподать напиток",
                    npc.transform
                );
            }
            else
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nпринять заказ",
                    npc.transform
                );
            }

            return;
        }

        GlassFocusTarget glassFocusTarget =
            hit.collider.GetComponentInParent<GlassFocusTarget>();

        if (glassFocusTarget != null)
        {
            ClearSlotHighlight();

            if (_heldObject.GetComponent<Garnish>() != null ||
                _heldObject.GetComponentInChildren<Garnish>() != null)
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nповесить украшение",
                    glassFocusTarget.transform
                );
            }
            else if (_heldObject.GetComponent<SpoonTool>() != null ||
                    _heldObject.GetComponentInChildren<SpoonTool>() != null)
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nмешать напиток",
                    glassFocusTarget.transform
                );
            }
            else
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nрежим стакана",
                    glassFocusTarget.transform
                );
            }

            return;
        }

        ItemSlot slot =
            hit.collider.GetComponentInParent<ItemSlot>();

        if (slot == null)
        {
            ClearSlotHighlight();
            ShowHeldObjectHint();
            return;
        }

        if (slot.CanAccept(_heldObject))
        {
            _hoveredSlot = slot;
            HighlightSlot(slot);

            ShowHintAboveObject(
                $"[E]\nпоставить {_heldObject.DisplayName}",
                slot.transform
            );

            return;
        }

        ClearSlotHighlight();
        ShowHeldObjectHint();

        Debug.LogWarning(
            $"Slot {slot.name} cannot accept {_heldObject.DisplayName}. " +
            $"Item type: {_heldObject.ItemType}"
        );
        
        Shaker shaker = hit.collider.GetComponentInParent<Shaker>();

        if (shaker != null)
        {
            ClearSlotHighlight();

            if (IsHoldingGlass())
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nнадеть шейкер",
                    shaker.transform
                );
            }
            else
            {
                ShowHintAboveObject(
                    "[ЛКМ]\nшейкер",
                    shaker.transform
                );
            }

            return;
        }
    }

    private void ShowHeldObjectHint()
    {
        if (uiManager == null || _heldObject == null || playerCamera == null)
            return;

        uiManager.ShowWorldHint(
            $"[E]\nбросить {_heldObject.DisplayName}",
            _heldObject.transform.position + Vector3.up * hintHeight,
            playerCamera
        );
    }

    private void TryPickUp()
    {
        if (_hoveredHoldable == null || holdPoint == null)
            return;

        _heldObject = _hoveredHoldable;
        _heldObject.PickUp(holdPoint);

        if (uiManager != null)
            uiManager.HideWorldHint();

        Debug.Log($"Picked up: {_heldObject.DisplayName}");
    }

    private void DropHeldObject()
    {
        if (_heldObject == null)
            return;

        Debug.Log($"Dropped: {_heldObject.DisplayName}");

        Vector3 throwDirection = playerCamera != null
            ? playerCamera.transform.forward
            : transform.forward;

        _heldObject.Throw(throwDirection, throwForce);
        _heldObject = null;

        _wasCameraFocused = false;

        ClearSlotHighlight();
        ClearHover();
    }

    private void PlaceHeldObjectInSlot()
    {
        if (_heldObject == null || _hoveredSlot == null)
            return;

        Debug.Log($"Trying to place {_heldObject.DisplayName} into {_hoveredSlot.name}");

        bool placed = _hoveredSlot.Place(_heldObject);

        if (!placed)
        {
            Debug.LogWarning("Object was not placed. Still holding it.");
            return;
        }

        _heldObject = null;
        _wasCameraFocused = false;

        ClearSlotHighlight();
        ClearHover();
    }

    private void ExitFocusMode()
    {
        if (_activeStirMiniGame != null)
        {
            _activeStirMiniGame.StopStirring();
            _activeStirMiniGame = null;
        }

        if (cameraFocusController != null)
            cameraFocusController.ClearFocus();

        if (_heldObject != null)
            _heldObject.ExitFocusPose();

        _wasCameraFocused = false;

        HideFocusHint();
    }

    private void UpdateHeldObjectFocusPose()
    {
        if (cameraFocusController == null)
            return;

        bool isFocusedNow = cameraFocusController.IsFocused;

        if (isFocusedNow == _wasCameraFocused)
            return;

        _wasCameraFocused = isFocusedNow;

        if (_heldObject == null)
            return;

        if (isFocusedNow)
            _heldObject.EnterFocusPose();
        else
            _heldObject.ExitFocusPose();
    }

    private bool IsHoldingSpoon()
    {
        if (_heldObject == null)
            return false;

        SpoonTool spoon = _heldObject.GetComponent<SpoonTool>();

        if (spoon == null)
            spoon = _heldObject.GetComponentInChildren<SpoonTool>();

        return spoon != null;
    }

    private bool IsHoldingPourableBottle()
    {
        if (_heldObject == null)
            return false;

        PourableBottle bottle = _heldObject.GetComponent<PourableBottle>();

        if (bottle == null)
            bottle = _heldObject.GetComponentInChildren<PourableBottle>();

        return bottle != null;
    }

    private bool PressedExitFocusKey()
    {
        return Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.A) ||
               Input.GetKeyDown(KeyCode.S) ||
               Input.GetKeyDown(KeyCode.D) ||
               Input.GetKeyDown(KeyCode.Space);
    }

    private void HighlightSlot(ItemSlot slot)
    {
        if (_lastHighlightedSlot == slot)
            return;

        ClearSlotHighlight();

        _lastHighlightedSlot = slot;
        _lastHighlightedSlot.SetHighlight(true);
    }

    private void ClearSlotHighlight()
    {
        if (_lastHighlightedSlot != null)
            _lastHighlightedSlot.SetHighlight(false);

        _lastHighlightedSlot = null;
        _hoveredSlot = null;
    }

    private void ShowHintAboveObject(string text, Transform target)
    {
        if (uiManager == null || target == null || playerCamera == null)
            return;

        Vector3 hintPosition = target.position + Vector3.up * hintHeight;
        uiManager.ShowWorldHint(text, hintPosition, playerCamera);
    }

    private void ClearHover()
    {
        _hoveredHoldable = null;
        _hoveredNPC = null;

        if (uiManager != null)
            uiManager.HideWorldHint();
    }

    private void HideFocusHint()
    {
        if (uiManager != null)
            uiManager.HideFocusHint();
    }
    private bool IsHoldingGlass()
    {
        return _heldObject != null && _heldObject.ItemType == ItemType.Glass;
    }

    private void ServeDrinkToNPC()
    {
        BartendingManager bartendingManager = FindFirstObjectByType<BartendingManager>();

        if (bartendingManager == null)
        {
            Debug.LogError("BartendingManager not found.");
            return;
        }

        Debug.Log("Trying to submit drink to NPC...");

        bartendingManager.SubmitDrink();
    }
}