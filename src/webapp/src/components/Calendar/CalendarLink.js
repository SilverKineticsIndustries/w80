import { memo, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { useNavigate } from 'react-router-dom';
import calendar from '../../assets/calendar.png';
import { UserContext } from '../../App';
import { printLocalizedShortDate } from '../../helpers/dates';

const styles = createUseStyles({
    wrapper: {
        whiteSpace: "nowrap",
        verticalAlign: "bottom",
    },
    appointmentIcon: {
        verticalAlign: "middle",
        marginRight: "8px"
    },
    appointmentButton: {
        cursor: "pointer",
        background: "transparent",
        border: "none"
    },
    appointmentTitle: {
        maxWidth: "170px",
        display: "inline-block",
        overflow: "hidden",
        textOverflow: "ellipsis",
        verticalAlign: "bottom"
    }
})

const CalendarLink = ({appointment}) => {

    const classes = styles();
    const navigator = useNavigate();
    const { currentUser } = useContext(UserContext);

    const openCalendarItem = () => {
        navigator('/calendar?id=' + encodeURIComponent(appointment.id));
    }

    return (
        <>
        {appointment &&
            <div className={classes.wrapper}>
                <img src={calendar} width="20px" height="20px" className={classes.appointmentIcon} alt='Calendar' />
                <button className={classes.appointmentButton} onClick={() => openCalendarItem()}>
                    {printLocalizedShortDate(currentUser.culture, appointment.startDateTimeUTC)} - <span className={classes.appointmentTitle}>{appointment.description}</span>
                </button>
            </div>
        }
        </>
    )
}

export default memo(CalendarLink);