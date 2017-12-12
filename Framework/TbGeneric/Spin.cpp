
#include "stdafx.h"

#include "spin.h"
#include "Globals.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
static const TCHAR BASED_CODE szClassName[] = _T("ItriSpinCtrl32");

//Timer identifiers.
#define IDT_FIRSTCLICK      500
#define IDT_HOLDCLICK       501

#define CTICKS_FIRSTCLICK   400
#define CTICKS_HOLDCLICK    50

//Default range and position constants.
#define IDEFAULTMIN         0
#define IDEFAULTMAX         9
#define IDEFAULTPOS         5



//Color indices for MSM_COLORSET/GET and MSCrColorSet/Get
#define MSCOLOR_FACE        0
#define MSCOLOR_ARROW       1
#define MSCOLOR_SHADOW      2
#define MSCOLOR_HIGHLIGHT   3
#define MSCOLOR_FRAME       4
#define CCOLORS             5

/*
 * In window extra bytes we simply store a local handle to
 * a MUSCROLL data structure.  The local memory is allocated
 * from the control's local heap (either application or DLL)
 * instead of from USER's heap, thereby saving system resources.
 *
 * Note that the window styles that are stored in the regular
 * windwow structure are copied here.  This is to optimize access
 * to these bits, avoiding extra calls to GetWindowLong.
*/

typedef struct tagMUSCROLL
    {
    HWND        hWndAssociate;  //Associate window handle
    DWORD       dwStyle;        //non serve piu'
    WORD        iMin;           //Minimum position
    WORD        iMax;           //Maximum position
    WORD        iPos;           //Current position
    WORD        wState;         //State flags
    COLORREF    rgCr[CCOLORS];  //Configurable colors.
    } MUSCROLL;

typedef MUSCROLL     *PMUSCROLL;
typedef MUSCROLL FAR *LPMUSCROLL;

//Extra bytes for the window if the size of a local handle.
#define CBWINDOWEXTRA       sizeof(PMUSCROLL)

//Extra Class bytes.
#define CBCLASSEXTRA        0

//Control state flags.
#define MUSTATE_GRAYED      0x0001
#define MUSTATE_HIDDEN      0x0002
#define MUSTATE_MOUSEOUT    0x0004
#define MUSTATE_UPCLICK     0x0008
#define MUSTATE_DOWNCLICK   0x0010
#define MUSTATE_LEFTCLICK   0x0008  //Repeated since MSS_VERTICAL and
#define MUSTATE_RIGHTCLICK  0x0010  //MSS_HORIZONTAL are exclusive.

//Combination of click states.
#define MUSTATE_CLICKED     (MUSTATE_LEFTCLICK | MUSTATE_RIGHTCLICK)

//Combination of state flags.
#define MUSTATE_ALL         0x001F
#define COMPATIBLE_GET_HANDLE(hWnd)	(HWND)GetWindowLong(hWnd, GWL_HWNDPARENT)
#define CBMUSCROLL sizeof(MUSCROLL)

// Macros to change the control state given a PMUSCROLL and state flag(s)
//
#define StateSet(p, wFlags)    (p->wState |=  (wFlags))
#define StateClear(p, wFlags)  (p->wState &= ~(wFlags))
#define StateTest(p, wFlags)   (p->wState &   (wFlags))

// colori di default usati per disegnare il control
const WORD rgColorDef[CCOLORS] =
	{
		COLOR_BTNFACE,
		COLOR_BTNTEXT,
		COLOR_BTNSHADOW,
		COLOR_BTNHIGHLIGHT,
		COLOR_WINDOWFRAME
	};

//Private functions specific to the control.
//-----------------------------------------------------------------------------
LONG SpinCtrlAPI		(HWND, UINT, WPARAM, LPARAM, PMUSCROLL);
LONG SpinCtrlPaint		(HWND, PMUSCROLL);
void PositionChange		(HWND, PMUSCROLL);
void ClickedRectCalc	(HWND, PMUSCROLL, LPRECT);

// Control window procedure
//-----------------------------------------------------------------------------
LONG WINAPI SpinCtrlWndProc	(HWND, UINT, WPARAM, LPARAM);


// Purpose:
//		Registers the SpinCtrl control class, including CS_GLOBALCLASS
//		to make the control available to all applications in the system.
//
//	Parameters:
//		HANDLE Instance of the application or DLL that will own this class.
//
//	Return Value:
//		TRUE if the class is registered, FALSE otherwise.
//		TRUE is also returned if the class was already registered.
//
//-----------------------------------------------------------------------------
BOOL RegisterSpinCtrl(HINSTANCE hInstance)
{
    WNDCLASS wc;

    if (GetClassInfo(hInstance, szClassName, &wc))
    	return TRUE;
   
    wc.lpfnWndProc   = SpinCtrlWndProc;
    wc.cbClsExtra    = CBCLASSEXTRA;
    wc.cbWndExtra    = CBWINDOWEXTRA;
    wc.hInstance     = hInstance;
    wc.hIcon         = (HICON)NULL;
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH) (COLOR_BTNFACE + 1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = szClassName;
    wc.style         = CS_DBLCLKS | CS_GLOBALCLASS |
                           CS_VREDRAW | CS_HREDRAW;
	return RegisterClass(&wc);
}

// SpinCtrlAPI
//
// Purpose:
//  Processes any control-specific function messages for the
//  SpinCtrl control.
//
// Parameters:
//  hWnd            HWND handle to the control window.
//  iMsg            UINT message identifier.
//  wParam          WPARAM parameter of the message.
//  lParam          LPARAM parameter of the message.
//  pMS             PMUSCROLL pointer to control-specific data.
//
// Return Value:
//  LONG            Varies with the message.
//
//-----------------------------------------------------------------------------
LONG SpinCtrlAPI(HWND hWnd, UINT iMsg, WPARAM wParam, LPARAM lParam, PMUSCROLL pMS)
{
    DWORD           dwT;
    COLORREF        cr;
    HWND            hWndT;
    WORD            iMin, iMax;
    WORD            iPos;

    switch (iMsg)
	{
        case MSM_HWNDASSOCIATESET:
            //Change the associate window of this control.
            if (!IsWindow((HWND)wParam))
                return -1;

            //Save old window handle.
            hWndT = pMS->hWndAssociate;
            pMS->hWndAssociate = (HWND)wParam;
            return (LONG)(DWORD)hWndT;


        case MSM_HWNDASSOCIATEGET:
            return (LONG)(DWORD)pMS->hWndAssociate;


        case MSM_DWRANGESET:
            // Set the new range, sending the appropriate notifications.
            // Also send a scroll message if the position has to change.
            // If the minimum is greater than the max, return error.
            // 
            if (LOWORD(lParam) >= HIWORD(lParam))
                return -1L;

            //Save old values.
            iMin = pMS->iMin;
            iMax = pMS->iMax;

            pMS->iMin = LOWORD(lParam);
            pMS->iMax = HIWORD(lParam);

            // If current position is outside of new range, force it to
            // the average of the range, otherwise leave it be.
            //
            if ((pMS->iMin >= pMS->iPos) ||
                (pMS->iMax <= pMS->iPos))
            {
                pMS->iPos = (pMS->iMin + pMS->iMax) / 2;

                //Send a scroll message if we change position.
                iMsg = (MSS_VERTICAL & pMS->dwStyle) ? WM_VSCROLL : WM_HSCROLL;
				wParam = (WPARAM)MAKELONG(SB_THUMBTRACK, pMS->iPos);
				lParam = (LPARAM)(hWnd);

                ::SendMessage(pMS->hWndAssociate, iMsg, wParam, lParam);

			}

            //Return old range.
            return MAKELONG(iMin, iMax);

        case MSM_DWRANGEGET:
            return MAKELONG(pMS->iMin, pMS->iMax);


        case MSM_WCURRENTPOSSET:
            // Set the new position if it falls within the valid range,
            // sending the appropriate scroll message.
            // 
            //Save current position
            iPos = pMS->iPos;

            if ((pMS->iMin <= wParam) && (pMS->iMax >= wParam))
			{
                pMS->iPos = wParam;
                iMsg = (MSS_VERTICAL & pMS->dwStyle) ? WM_VSCROLL : WM_HSCROLL;
				wParam = (WPARAM)MAKELONG(SB_THUMBTRACK, pMS->iPos);
				lParam = (LPARAM)(hWnd);

                ::SendMessage(pMS->hWndAssociate, iMsg, wParam, lParam);

                //Return old position.
                return MAKELONG(iPos, 0);
            }

            //Invalid position.
            return -1;

        case MSM_WCURRENTPOSGET:
            return MAKELONG(pMS->iPos, 0);


        case MSM_FNOPEGSCROLLSET:
            // Set the MSS_NOPEGSCROLL style to the value in
            // lParam which is zero or MSS_NOPEGSCROLL.
            // 
            dwT = pMS->dwStyle & MSS_NOPEGSCROLL;

            //Either set of clear the style.
            if ((BOOL)wParam)
                pMS->dwStyle |= MSS_NOPEGSCROLL;
            else
                pMS->dwStyle &= ~MSS_NOPEGSCROLL;

            //Return TRUE or FALSE if the bit was or wasn't set
            return (dwT ? 1L : 0L);

        case MSM_FNOPEGSCROLLGET:
            return (pMS->dwStyle & MSS_NOPEGSCROLL);


        case MSM_FINVERTRANGESET:
            // Set the MSS_INVERTRANGE style to the value in
            // lParam which is zero or MSS_INVERTRANGE.
            //  
            dwT = pMS->dwStyle & MSS_INVERTRANGE;

            //Either set of clear the style.
            if ((BOOL)wParam)
                pMS->dwStyle |= MSS_INVERTRANGE;
            else
                pMS->dwStyle &= ~MSS_INVERTRANGE;

            //Return TRUE or FALSE if the bit was or wasn't set
            return (dwT ? 1L : 0L);


        case MSM_FINVERTRANGEGET:
            return (pMS->dwStyle & MSS_INVERTRANGE);


        case MSM_CRCOLORSET:
            if (wParam >= CCOLORS)
                return 0L;

            cr = pMS->rgCr[wParam];

            //If -1 is set in rgCr the paint procedure uses a default.
            pMS->rgCr[wParam] = (COLORREF)lParam;

            //Force repaint since we changed a state.
            InvalidateRect(hWnd, NULL, TRUE);
            UpdateWindow(hWnd);

            //Return the old color.
            return (LONG)cr;

        case MSM_CRCOLORGET:
            if (wParam >= CCOLORS)
                return 0L;

            return (LONG)pMS->rgCr[wParam];
	}
    return 0L;
}

// SpinCtrlWndProc
//
// Purpose:
//  Window Procedure for the SpinCtrl custom control.  Handles all
//  messages like WM_PAINT just as a normal application window would.
//  Any message not processed here should go to DefWindowProc.
//
// Parameters:
//  hWnd            HWND handle to the control window.
//  iMsg            WORD message identifier.
//  wParam          WPARAM parameter of the message.
//  lParam          LPARAM parameter of the message.
//
// Return Value:
//  LONG            Varies with the message.
//
//-----------------------------------------------------------------------------
LONG WINAPI SpinCtrlWndProc(HWND hWnd, UINT iMsg, WPARAM wParam, LPARAM lParam)
{
    PMUSCROLL       pMS;
    POINT           pt;
    RECT            rect;
    int			    x, y;
    int		        cx, cy;
    WORD            wState;


    // Get a pointer to the MUSCROLL structure for this control.
    // Note that if we do this before WM_NCCREATE where we allocate
    // the memory, pMS will be NULL, which is not a problem since
    // we do not access it until after WM_NCCREATE.
    // 
    pMS = (PMUSCROLL)GetWindowLong(hWnd, 0);

    //Let the API handler process WM_USER+xxxx messages
    if (iMsg >= WM_USER)
        return SpinCtrlAPI(hWnd, iMsg, wParam, lParam, pMS);


    //Handle standard Windows messages.
    switch (iMsg)
    {
        case WM_NCCREATE:
		{
			pMS = new MUSCROLL;
			memset(pMS, 0, CBMUSCROLL);

	        if (!pMS)
	            return 0L;
		
	        SetWindowLong(hWnd, 0, (LONG)pMS);
	        return 1L;
	    }

        case WM_CREATE:
		{
	        //Our associate is the parent by default.
			pMS->hWndAssociate = COMPATIBLE_GET_HANDLE(hWnd);
		
	        //Copy styles
	        pMS->dwStyle = ((LPCREATESTRUCT)lParam)->style;
		
	        // Enforce exclusive MSS_VERTICAL and MSS_HORIZONTAL,
	        // defaulting to MSS_VERTICAL.
	        //
	        if ((MSS_VERTICAL & pMS->dwStyle) && (MSS_HORIZONTAL & pMS->dwStyle))
	            pMS->dwStyle &= ~MSS_HORIZONTAL;
		
	        // Use defaults 
	        pMS->iMin = IDEFAULTMIN;
	        pMS->iMax = IDEFAULTMAX;
	        pMS->iPos = IDEFAULTPOS;
		                  
	        //Clear out all initial states.
	        StateClear(pMS, MUSTATE_ALL);
		
	        //Indicate that all colors are defaults.
	        for (int i = 0; i < CCOLORS; i++)
	            pMS->rgCr[i] = (COLORREF)-1L;
	        return 1L;
	    }

        case WM_NCDESTROY:
            //Free the control's memory.
            ASSERT_TRACE(pMS,"pointer to the MUSCROLL structure cannot be null in this context");
            if (pMS) delete pMS;
            break;

        case WM_ERASEBKGND:
            // Eat this message to avoid erasing portions that
            // we are going to repaint in WM_PAINT.  Part of a
            // change-state-and-repaint strategy is to rely on
            // WM_PAINT to do anything visual, which includes
            // erasing invalid portions.  Letting WM_ERASEBKGND
            // erase the background is redundant.
            break;


        case WM_PAINT:
            return SpinCtrlPaint(hWnd, pMS);


        case WM_ENABLE:
            // Handles disabling/enabling case.  Example of a
            // change-state-and-repaint strategy since we let the
            // painting code take care of the visuals.
			//
			if (wParam)
                StateClear(pMS, MUSTATE_GRAYED);
            else
                StateSet(pMS, MUSTATE_GRAYED);

            //Force a repaint since the control will look different.
            InvalidateRect(hWnd, NULL, TRUE);
            UpdateWindow(hWnd);
            break;


        case WM_SHOWWINDOW:
            // Set or clear the hidden flag. Windows will
            // automatically force a repaint if we become visible.
            // 
            if (wParam)
                StateClear(pMS, MUSTATE_HIDDEN);
            else
                StateSet(pMS, MUSTATE_HIDDEN);

            break;


        case WM_CANCELMODE:
            // IMPORTANT MESSAGE!  WM_CANCELMODE means that a
            // dialog or some other modal process has started.
            // we must make sure that we cancel any clicked state
            // we are in, kill the timers, and release the capture.
            ///
            StateClear(pMS, MUSTATE_DOWNCLICK | MUSTATE_UPCLICK);
            KillTimer(hWnd, IDT_FIRSTCLICK);
            KillTimer(hWnd, IDT_HOLDCLICK);
            ReleaseCapture();
            break;


        case WM_TIMER:
        	// se non siamo in stato di mouse clickato non interessa
            if (!StateTest(pMS, MUSTATE_CLICKED))
                break;

            // We run two timers:  the first is the initial delay
            // after the first click before we begin repeating, the
            // second is the repeat rate.
            // 
            if (wParam == IDT_FIRSTCLICK)
			{
                KillTimer(hWnd, wParam);
                SetTimer(hWnd, IDT_HOLDCLICK, CTICKS_HOLDCLICK, NULL);
            }

            // Send a new scroll message if the mouse is still in the
            // originally clicked area.
            // 
            if	(
            		(wParam == IDT_FIRSTCLICK || wParam == IDT_HOLDCLICK) &&
            		!StateTest(pMS, MUSTATE_MOUSEOUT)
            	)
                PositionChange(hWnd, pMS);

            break;


        case WM_LBUTTONDBLCLK:
        case WM_LBUTTONDOWN:
            // When we get a mouse down message, we know that the mouse
            // is over the control.  We then do the following steps
            // to set up the new state:
            //  1.  Hit-test the coordinates of the click to
            //      determine in which half the click occurred.
            //  2.  Set the appropriate MUSTATE_*CLICK state
            //      and repaint that clicked half.  This is another
            //      example of a change-state-and-repaint strategy.
            //  3.  Send an initial scroll message.
            //  4.  Set the mouse capture.
            //  5.  Set the initial delay timer before repeating
            //      the scroll message.
            //
            // A WM_LBUTTONDBLCLK message means that the user clicked
            // the control twice in succession which we want to treat
            // like WM_LBUTTONDOWN.  This is safe since we will receive
            // WM_LBUTTONUP before the WM_LBUTTONDBLCLK.
            // 
            //Get the mouse coordinates.
            x=LOWORD(lParam);
            y=HIWORD(lParam);

            // Only need to hit-test the upper half for a vertical
            // control or the left half for a horizontal control.
            // 
            CWnd::FromHandle(hWnd)->GetClientRect(&rect);
            cx = rect.right  / 2;
            cy = rect.bottom / 2;

            if (MSS_VERTICAL & pMS->dwStyle)
                wState = (y > cy) ? MUSTATE_DOWNCLICK : MUSTATE_UPCLICK;
            else
                wState = (x > cx) ? MUSTATE_RIGHTCLICK : MUSTATE_LEFTCLICK;

            //Change-state-and-repaint
            StateSet(pMS, wState);
            ClickedRectCalc(hWnd, pMS, &rect);
            InvalidateRect(hWnd, &rect, TRUE);
            UpdateWindow(hWnd);

            PositionChange(hWnd, pMS);
            SetCapture(hWnd);
            SetTimer(hWnd, IDT_FIRSTCLICK, CTICKS_FIRSTCLICK, NULL);
            break;


        case WM_MOUSEMOVE:
            // On WM_MOUSEMOVE messages we want to know if the mouse
            // has moved out of the control when the control is in
            // a clicked state.  If the control has not been clicked,
            // then we have nothing to do.  Otherwise we want to set
            // the MUSTATE_MOUSEOUT flag and repaint so the button
            // visually comes up.
            ///
            if (!StateTest(pMS, MUSTATE_CLICKED))
                break;


            //Get the area we originally clicked and the new POINT
            ClickedRectCalc(hWnd, pMS, &rect);
            pt.x = LOWORD(lParam);
            pt.y = HIWORD(lParam);

            wState = pMS->wState;

            //Hit-Test the rectange and change the state if necessary.
            if (PtInRect(&rect, pt))
                StateClear(pMS, MUSTATE_MOUSEOUT);
            else
                StateSet(pMS, MUSTATE_MOUSEOUT);

            // If the state changed, repaint the appropriate part of
            // the control.
            // 
            if (wState != pMS->wState)
			{
                InvalidateRect(hWnd, &rect, TRUE);
                UpdateWindow(hWnd);
            }
            break;


        case WM_LBUTTONUP:
            // A mouse button up event is much like WM_CANCELMODE since
            // we have to clean out whatever state the control is in:
            //  1.  Kill any repeat timers we might have created.
            //  2.  Release the mouse capture.
            //  3.  Clear the clicked states and repaint, another example
            //      of a change-state-and-repaint strategy.
            // 
            KillTimer(hWnd, IDT_FIRSTCLICK);
            KillTimer(hWnd, IDT_HOLDCLICK);

            ReleaseCapture();

            // Repaint if necessary, only if we are clicked AND the mouse
            // is still in the boundaries of the control.
            //  
            if (StateTest(pMS, MUSTATE_CLICKED) &&
                StateTest(pMS, ~MUSTATE_MOUSEOUT))
            {
                //Calculate the rectangle before clearing states.
                ClickedRectCalc(hWnd, pMS, &rect);

                //Clear the states so we repaint properly.
                StateClear(pMS, MUSTATE_MOUSEOUT);
                StateClear(pMS, MUSTATE_CLICKED);


                InvalidateRect(hWnd, &rect, TRUE);
                UpdateWindow(hWnd);
            }

			// signal parent mouse up event for later processing
	        ::PostMessage(pMS->hWndAssociate, iMsg, wParam, lParam);
	        
            //Insure that we clear out the states.
            break;


        default:
			{
				return ::DefWindowProc(hWnd, iMsg, wParam, lParam);
			}
    }

    return 0L;
}



// ClickedRectCalc
//
// Purpose:
//  Calculates the rectangle of the clicked region based on the
//  state flags MUSTATE_UPCLICK, MUSTATE_DOWNCLICK, MUSTATE_LEFTCLICK,
//  and MUSTATE_RIGHTLICK, depending on the style.
//
// Parameter:
//  hWnd            HWND handle to the control window.
//  lpRect          LPRECT rectangle structure to fill.
//
//-----------------------------------------------------------------------------
void ClickedRectCalc(HWND hWnd, PMUSCROLL pMS, LPRECT lpRect)
{
    int cx, cy;

	CWnd::FromHandle(hWnd)->GetClientRect(lpRect);
    cx = lpRect->right  / 2;
    cy = lpRect->bottom / 2;

    if (MSS_VERTICAL & pMS->dwStyle)
	{
        if (StateTest(pMS, MUSTATE_DOWNCLICK))
            lpRect->top=cy;

        if (StateTest(pMS, MUSTATE_UPCLICK))
            lpRect->bottom=1+cy;
    }
    else
    {
        //MSS_HORIZONTAL
        if (StateTest(pMS, MUSTATE_RIGHTCLICK))
            lpRect->left=cx;

        if (StateTest(pMS, MUSTATE_LEFTCLICK))
            lpRect->right=1+cx;
    }

    return;
}

// PositionChange
//
// Purpose:
//  Checks what part of the control is clicked, modifies the current
//  position accordingly (taking MSS_INVERTRANGE into account) and
//  sends an appropriate message to the associate.  For MSS_VERTICAL
//  controls we send WM_VSCROLL messages and for MSS_HORIZONTAL controls
//  we send WM_HSCROLL.
//
//  The scroll code in the message is always SB_LINEUP for the upper
//  or left half of the control (vertical and horizontal, respectively)
//  and SB_LINEDOWN for the bottom or right half.
//
//  This function does not send a message if the position is pegged
//  on the minimum or maximum of the range if MSS_NOPEGSCROLL is
//  set in the style bits.
//
// Parameters:
//  hWnd            HWND of the control.
//  pMS             PMUSCROLL pointer to control data structure.
//
// Return Value:
//  void
//
//-----------------------------------------------------------------------------
void PositionChange(HWND hWnd, PMUSCROLL pMS)
{
    WORD         wScrollMsg;
    WORD         wScrollCode;
    BOOL         fPegged=FALSE;

    if (StateTest(pMS, MUSTATE_UPCLICK | MUSTATE_LEFTCLICK))
        wScrollCode=SB_LINEUP;

    if (StateTest(pMS, MUSTATE_DOWNCLICK | MUSTATE_RIGHTCLICK))
        wScrollCode=SB_LINEDOWN;

    wScrollMsg=(MSS_VERTICAL & pMS->dwStyle) ? WM_VSCROLL : WM_HSCROLL;

    // Modify the current position according to the following rules:
    //
    // 1. On SB_LINEUP with an inverted range, increment the position.
    //    If the position is already at the maximum, set the pegged flag.
    //
    // 2. On SB_LINEUP with an normal range, decrement the position.
    //    If the position is already at the minimum, set the pegged flag.
    //
    // 3. On SB_LINEDOWN with an inverted range, treat like SB_LINEUP
    //    with a normal range.
    //
    // 4. On SB_LINEDOWN with an normal range, treat like SB_LINEUP
    //    with an inverted range.
    //
    if (wScrollCode == SB_LINEUP)
    {
        if (MSS_INVERTRANGE & pMS->dwStyle)
        {
            if (pMS->iPos == pMS->iMax)
                fPegged = TRUE;
            else
                pMS->iPos++;
        }
		else
        {
            if (pMS->iPos == pMS->iMin)
                fPegged=TRUE;
            else
                pMS->iPos--;
        }
    }
    else
    {
        if (MSS_INVERTRANGE & pMS->dwStyle)
        {
            if (pMS->iPos == pMS->iMin)
                fPegged = TRUE;
            else
                pMS->iPos--;
        }
        else
        {
            if (pMS->iPos==pMS->iMax)
                fPegged=TRUE;
            else
                pMS->iPos++;
        }
    }


    // Send a message if we changed and are not pegged, or did not change
    // and MSS_NOPEGSCROLL is clear.
    // 
    if (!fPegged || !(MSS_NOPEGSCROLL & pMS->dwStyle))
    {
		WPARAM wParam = (WPARAM)MAKELONG(wScrollCode, pMS->iPos);
		LPARAM lParam = (LPARAM)(hWnd);
        ::SendMessage(pMS->hWndAssociate, wScrollMsg, wParam, lParam);
    }

    return;
}

// Draw3DButtonRect
//
// Purpose:
//  Draws the 3D button look within a given rectangle.  This rectangle
//  is assumed to be bounded by a one pixel black border, so everything
//  is bumped in by one.
//
// Parameters:
//  hDC         DC to draw to.
//  hPenHigh    HPEN highlight color pen.
//  hPenShadow  HPEN shadow color pen.
//  x1          WORD Upper left corner x.
//  y1          WORD Upper left corner y.
//  x2          WORD Lower right corner x.
//  y2          WORD Lower right corner y.
//  fClicked    BOOL specifies if the button is down or not (TRUE==DOWN)
//
//
//-----------------------------------------------------------------------------
void Draw3DButtonRect(HDC hDC, HPEN hPenHigh, HPEN hPenShadow,
                             int x1, int y1, int x2, int y2,
                             BOOL fClicked)
{
    HPEN        hPenOrg;

    //Shrink the rectangle to account for borders.
    x1 += 1;
    x2 -= 1;
    y1 += 1;
    y2 -= 1;

    hPenOrg = (HPEN)::SelectObject(hDC, hPenShadow);

    if (fClicked)
	{
        //Shadow on left and top edge when clicked.
        MoveToEx(hDC, x1, y2, NULL);
        LineTo  (hDC, x1, y1);
        LineTo  (hDC, x2+1, y1);
    }
    else
    {
        //Lowest shadow line.
        MoveToEx(hDC, x1, y2, NULL);
        LineTo  (hDC, x2, y2);
        LineTo  (hDC, x2, y1 - 1);

        SelectObject(hDC, hPenHigh);

        //Upper highlight line.
        MoveToEx(hDC, x1,   y2 - 1, NULL);
        LineTo  (hDC, x1,   y1);
        LineTo  (hDC, x2,   y1);
	}

    SelectObject(hDC, hPenOrg);
    return;
}

// SpinCtrlPaint
//
// Purpose:
//  Handles all WM_PAINT messages for the control and paints
//  the control for the current state, whether it be clicked
//  or disabled.
//
//  PLEASE NOTE!
//  This painting routine makes no attempt at optimizations
//  and is intended for demonstration and education.
//
// Parameters:
//  hWnd            HWND Handle to the control.
//  pMS             PMUSCROLL control data pointer.
//
// Return Value:
//  LONG            0L.
// 
//
//-----------------------------------------------------------------------------
LONG SpinCtrlPaint(HWND hWnd, PMUSCROLL pMS)
{
    PAINTSTRUCT ps;
    LPRECT      lpRect;
    RECT        rect;
    HDC         hDC;
    COLORREF    rgCr[CCOLORS];
    HPEN        rgHPen[CCOLORS];
    WORD        iColor;

    HBRUSH      hBrushArrow;
    HBRUSH      hBrushFace;

    POINT       rgpt1[3];
    POINT       rgpt2[3];

    WORD        xAdd1=0, yAdd1=0;
    WORD        xAdd2=0, yAdd2=0;

    int        cx,  cy;    //Whole dimensions
    int        cx2, cy2;   //Half dimensions
    int        cx4, cy4;   //Quarter dimensions
    int        xoffs;	   //Arrows dimensions
    int        yoffs;	   //Arrows dimensions


    lpRect=&rect;

    hDC=BeginPaint(hWnd, &ps);
	CWnd::FromHandle(hWnd)->GetClientRect(lpRect);

    // Get colors that we'll need.  We do not want to cache these
    // items since we may our top-level parent window may have
    // received a WM_WININICHANGE message at which time the control
    // is repainted.  Since this control never sees that message,
    // we cannot assume that colors will remain the same throughout
    // the life of the control.
    //
    // We use the system color if pMS->rgCr[i] is -1, otherwise we
    // use the color in pMS->rgCr[i].
    //  
    for (iColor=0; iColor < CCOLORS; iColor++)
	{
        if (-1L == pMS->rgCr[iColor])
            rgCr[iColor]=GetSysColor(rgColorDef[iColor]);
        else
            rgCr[iColor]=pMS->rgCr[iColor];

        rgHPen[iColor]=CreatePen(PS_SOLID, 1, rgCr[iColor]);
	}

    hBrushFace =CreateSolidBrush(rgCr[MSCOLOR_FACE]);
    hBrushArrow=CreateSolidBrush(rgCr[MSCOLOR_ARROW]);

    // These values are extremely cheap to calculate for the amount
    // we are going to use them.
    // 
    cx = lpRect->right  - lpRect->left;
    cy = lpRect->bottom - lpRect->top;
    cx2 = cx  / 2;
    cy2 = cy  / 2;
    cx4 = cx2 / 2;
    cy4 = cy2 / 2;

    // If one half is depressed, set the x/yAdd variables that we use
    // to shift the small arrow image down and right.
    // 
    if (!StateTest(pMS, MUSTATE_MOUSEOUT))
    {
        if (StateTest(pMS, MUSTATE_UPCLICK | MUSTATE_LEFTCLICK))
        {
            xAdd1 = 1;
            yAdd1 = 1;
		}

        if (StateTest(pMS, MUSTATE_DOWNCLICK | MUSTATE_RIGHTCLICK))
        {
            xAdd2 = 1; 
            yAdd2 = 1;
        }
    }


    //Draw the face color and the outer frame
    SelectObject(hDC, hBrushFace);
    SelectObject(hDC, rgHPen[MSCOLOR_FRAME]);
    Rectangle(hDC, lpRect->left, lpRect->top, lpRect->right, lpRect->bottom);


    //Draw the arrows depending on the orientation.
    if (MSS_VERTICAL & pMS->dwStyle)
	{
		xoffs = MulDiv((cx > cy2 ? cy2 : cx), 2, 7);
		yoffs = MulDiv((cx > cy2 ? cy2 : cx), 2, 7) - 1;

        //Draw the horizontal center line.
        MoveToEx(hDC, 0,  cy2, NULL);
        LineTo  (hDC, cx, cy2);

        // We do one of three modifications for drawing the borders:
        //  1) Both halves un-clicked.
        //  2) Top clicked,   bottom unclicked.
        //  3) Top unclicked, bottom clicked.
        //
        // Case 1 is xAdd1==xAdd2==0
        // Case 2 is xAdd1==1, xAdd2=0
        // Case 3 is xAdd1==0, xAdd2==1
        //
        // 
        //Draw top and bottom buttons borders.
        Draw3DButtonRect(hDC, rgHPen[MSCOLOR_HIGHLIGHT],
                         rgHPen[MSCOLOR_SHADOW],
                         0,  0,  cx - 1, cy2,  xAdd1);

        Draw3DButtonRect(hDC, rgHPen[MSCOLOR_HIGHLIGHT],
                         rgHPen[MSCOLOR_SHADOW],
                         0, cy2, cx - 1, cy - 1, xAdd2);


        //Select default line color.
        SelectObject(hDC, rgHPen[MSCOLOR_ARROW]);

        //Draw the arrows depending on the enable state.
        if (StateTest(pMS, MUSTATE_GRAYED))
        {
            // Draw arrow color lines in the upper left of the
            // top arrow and on the top of the bottom arrow.
            // Pen was already selected as a default.
            // 
            MoveToEx(hDC, cx2,   cy4 - yoffs, NULL);      //Top arrow
            LineTo  (hDC, cx2 - xoffs, cy4 + yoffs);
            MoveToEx(hDC, cx2 - xoffs, cy2 + cy4 - yoffs, NULL);  //Bottom arrow
            LineTo  (hDC, cx2 + xoffs, cy2 + cy4 - yoffs);

            // Draw highlight color lines in the bottom of the
            // top arrow and on the ;pwer right of the bottom arrow.
            // 
            SelectObject(hDC, rgHPen[MSCOLOR_HIGHLIGHT]);
            MoveToEx(hDC,   cx2 - xoffs, cy4 + yoffs, NULL);      //Top arrow
            LineTo  (hDC,   cx2 + xoffs, cy4 + yoffs);
            MoveToEx(hDC,   cx2 + xoffs, cy2 + cy4 - yoffs, NULL);  //Bottom arrow
            LineTo  (hDC,   cx2,   cy2 + cy4 + yoffs);
            SetPixel(hDC, cx2,   cy2 + cy4 + yoffs, rgCr[MSCOLOR_HIGHLIGHT]);
        }
        else
        {
            //Top arrow polygon
            rgpt1[0].x = xAdd1 + cx2;
            rgpt1[0].y = yAdd1 + cy4 - yoffs;
            rgpt1[1].x = xAdd1 + cx2 - xoffs;
            rgpt1[1].y = yAdd1 + cy4 + yoffs;
            rgpt1[2].x = xAdd1 + cx2 + xoffs;
            rgpt1[2].y = yAdd1 + cy4 + yoffs;

            //Bottom arrow polygon
            rgpt2[0].x = xAdd2 + cx2;
            rgpt2[0].y = yAdd2 + cy2 + cy4 + yoffs;
            rgpt2[1].x = xAdd2 + cx2 - xoffs;
            rgpt2[1].y = yAdd2 + cy2 + cy4 - yoffs;
            rgpt2[2].x = xAdd2 + cx2 + xoffs;
            rgpt2[2].y = yAdd2 + cy2 + cy4 - yoffs;

            //Draw the arrows
            SelectObject(hDC, hBrushArrow);
            Polygon(hDC, (LPPOINT)rgpt1, 3);
            Polygon(hDC, (LPPOINT)rgpt2, 3);
        }
    }
    else
    {
		yoffs = MulDiv((cy > cx2 ? cx2 : cy), 2, 7);
		xoffs = MulDiv((cy > cx2 ? cx2 : cy), 2, 7) - 1;

        //Draw the vertical center line, assume the frame color is selected.
        MoveToEx(hDC, cx2, 0, NULL);
        LineTo  (hDC, cx2, cy);

        // We do one of three modifications for drawing the borders:
        //  1) Both halves un-clicked.
        //  2) Left clicked,   right unclicked.
        //  3) Left unclicked, right clicked.
        //
        // Case 1 is xAdd1==xAdd2==0
        // Case 2 is xAdd1==1, xAdd2=0
        // Case 3 is xAdd1==0, xAdd2==1
        //
        //Draw left and right buttons borders.
        Draw3DButtonRect(hDC, rgHPen[MSCOLOR_HIGHLIGHT],
                         rgHPen[MSCOLOR_SHADOW],
                         0,   0, cx2,  cy - 1, xAdd1);

        Draw3DButtonRect(hDC, rgHPen[MSCOLOR_HIGHLIGHT],
                         rgHPen[MSCOLOR_SHADOW],
                         cx2, 0, cx - 1, cy - 1, xAdd2);


        //Select default line color.
        SelectObject(hDC, rgHPen[MSCOLOR_ARROW]);

        //Draw the arrows depending on the enable state.
        if (StateTest(pMS, MUSTATE_GRAYED))
        {
            // Draw arrow color lines in the upper left of the
            // left arrow and on the left of the right arrow.
            // Pen was already selected as a default.
            // 
            MoveToEx(hDC, cx4 - xoffs,       cy2, NULL);          //Left arrow
            LineTo  (hDC, cx4 + xoffs,       cy2 - yoffs);
            MoveToEx(hDC, cx2 + cx4 - xoffs, cy2 - yoffs, NULL);      //Right arrow
            LineTo  (hDC, cx2 + cx4 - xoffs, cy2 + yoffs);

            // Draw highlight color lines in the bottom of the
            // top arrow and on the ;pwer right of the bottom arrow.
            // 
            SelectObject(hDC, rgHPen[MSCOLOR_HIGHLIGHT]);
            MoveToEx(hDC, cx4 + xoffs,       cy2 - yoffs, NULL);
            LineTo  (hDC, cx4 + xoffs,       cy2 + yoffs);
            MoveToEx(hDC, cx2 + cx4 + xoffs, cy2, NULL);
            LineTo  (hDC, cx2 + cx4 - xoffs, cy2 + yoffs);
        }
        else
        {
            //Left arrow polygon
            rgpt1[0].x = xAdd1 + cx4 - xoffs;
            rgpt1[0].y = yAdd1 + cy2;
            rgpt1[1].x = xAdd1 + cx4 + xoffs;
            rgpt1[1].y = yAdd1 + cy2 + yoffs;
            rgpt1[2].x = xAdd1 + cx4 + xoffs;
            rgpt1[2].y = yAdd1 + cy2 - yoffs;

            //Right arrow polygon
            rgpt2[0].x = xAdd2 + cx2 + cx4 + xoffs;
            rgpt2[0].y = yAdd2 + cy2;
            rgpt2[1].x = xAdd2 + cx2 + cx4 - xoffs;
            rgpt2[1].y = yAdd2 + cy2 + yoffs;
            rgpt2[2].x = xAdd2 + cx2 + cx4 - xoffs;
            rgpt2[2].y = yAdd2 + cy2 - yoffs;

            //Draw the arrows
            SelectObject(hDC, hBrushArrow);
            Polygon(hDC, (LPPOINT)rgpt1, 3);
            Polygon(hDC, (LPPOINT)rgpt2, 3);
        }
    }

    //Clean up
    EndPaint(hWnd, &ps);

    DeleteObject(hBrushFace);
    DeleteObject(hBrushArrow);

    for (iColor = 0; iColor < CCOLORS; iColor++)
        DeleteObject(rgHPen[iColor]);

    return 0L;
}



/////////////////////////////////////////////////////////////////////////////
//						CSpin class wrapper implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSpin, CWnd)

//-----------------------------------------------------------------------------
CSpin::CSpin()
{ }

//-----------------------------------------------------------------------------
CWnd* CSpin::GetAssociate()
	{ return CWnd::FromHandle((HWND)SendMessage(MSM_HWNDASSOCIATEGET)); }

//-----------------------------------------------------------------------------
void CSpin::SetAssociate(CWnd* pNew)
	{ SendMessage(MSM_HWNDASSOCIATESET, (UINT)pNew->GetSafeHwnd()); }

//-----------------------------------------------------------------------------
void CSpin::GetRange(WORD& nMin, WORD& nMax)
	{
		DWORD dw = SendMessage(MSM_DWRANGEGET);
		nMin = LOWORD(dw); nMax = HIWORD(dw);
	}

//-----------------------------------------------------------------------------
void CSpin::SetRange(WORD nMin, WORD nMax)
	{ SendMessage(MSM_DWRANGESET, 0, MAKELONG(nMin, nMax)); }

//-----------------------------------------------------------------------------
UINT CSpin::GetCurrentPos()
	{ return (UINT)SendMessage(MSM_WCURRENTPOSGET); }

//-----------------------------------------------------------------------------
void CSpin::SetCurrentPos(UINT nPos)
	{ SendMessage(MSM_WCURRENTPOSSET, nPos); }

//-----------------------------------------------------------------------------
BOOL CSpin::Create(DWORD dwStyle, const RECT& rect, CWnd* pWnd, UINT nID)
{
	return CWnd::Create(szClassName, NULL, dwStyle, rect, pWnd, nID);
}

