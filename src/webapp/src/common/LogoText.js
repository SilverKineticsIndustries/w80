import React, {memo} from 'react';
import { createUseStyles } from 'react-jss'

const styles = createUseStyles({
    wrapper: {
        fontSize: ".85em"
    }
})

const LogoText = () => {
    const classes = styles();
    return (
        <span className={classes.wrapper}>
            [Silver Kinetics Industries]
        </span>
    )
}

export default memo(LogoText);