import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';

const styles = createUseStyles({
    circle: {
        height: "45px",
        width: "45px",
        borderRadius: "50%",
        margin: "10px",
        display: "inline-block",
        filter: "drop-shadow(0 0 0.20rem #252525)"
    },
})

const StateCircle = ({color}) =>
{
    const classes = styles();
    const colorValue = color.indexOf("var") === -1 ? "#" + color : color;
    return(
        <div className={classes.circle} style={{ backgroundColor: colorValue }} />
    )
}

export default memo(StateCircle);