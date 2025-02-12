## PLAN OF IMPLEMENTATION FOR ANALYSIS AND PLOTTING OF OPTITRACK USER DATA
## Description:
##  The following code is divided into two parts:

##  The first part is the analysis part, which aims to 
##  do one major thing: analyze the data collected 
##  by OptiTrack in .csv files. It analyzes and determines, 
##  with the passage of time, whether or not the data shows the 
##  user turning his/her head, and in which position (pitch, yaw, roll, etc.).

##  The second part is the plotting part, where we mainly use the ggplot2 
##  library to draw all the graphs.


##################################################################################################
## PART 1: ANALYSIS OF DATA
##################################################################################################

## PLAN 1 (abandoned, so not complete)

## Method: Compare two adjacent data points and calculate 
##  their difference. If the difference is larger than our
##  self-defined threshold, then we count it as a turn
##  of the head.
## Reason for abandonment: It's very difficult to find
##  a consistent threshold to differentiate between
##  valid movements and invalid ones. Small changes 
##  can make a big difference.
vdl_temp <- bs_valid_data_length
bs_valid_data_length <- 0
i <- 1
bs_data_count <- c(0, 0, 0, 0, 0 ,0 ,0)
differ <- 0.1

## 1 for pitch; 2 for yaw; 3 for roll; 
## 4 for pitch + yaw; 5 for pitch + roll;
## 6 for yaw + roll; 7 for pitch + yaw + roll
valid_length <- bs_valid_data_length
bs_pitch_data <- vector("numeric", length = valid_length)
bs_yaw_data <- vector("numeric", length = valid_length)
bs_roll_data <- vector("numeric", length = valid_length)

j <- 1
current_time <- bs_time[1]
record_idx <- 1

while (i < vdl_temp) {
  if (abs(current_time - bs_time[i]) >= 0.1) {
    current_time <- bs_time[i]
    if (abs(bs_pitch_cpy[i] - bs_pitch_cpy[record_idx]) > differ |
        abs(bs_yaw_cpy[i] - bs_yaw_cpy[record_idx]) > differ |
        abs(bs_roll_cpy[i] - bs_roll_cpy[record_idx]) > differ) {
      if (abs(bs_pitch_cpy[i] - bs_pitch_cpy[record_idx]) > differ &&
          abs(bs_yaw_cpy[i] - bs_yaw_cpy[record_idx]) > differ &&
          abs(bs_roll_cpy[i] - bs_roll_cpy[record_idx]) > differ) {
        bs_data_count[7] <- bs_data_count[7] + 1
      } 
      else if (abs(bs_pitch_cpy[i] - bs_pitch_cpy[record_idx]) > differ &&
              abs(bs_yaw_cpy[i] - bs_yaw_cpy[record_idx]) > differ) {
        bs_data_count[4] <- bs_data_count[4] + 1
      } 
      else if (abs(bs_pitch_cpy[i] - bs_pitch_cpy[record_idx]) > differ &&
              abs(bs_roll_cpy[i] - bs_roll_cpy[record_idx]) > differ) {
        bs_data_count[5] <- bs_data_count[5] + 1
      } 
      else if (abs(bs_yaw_cpy[i] - bs_yaw_cpy[record_idx]) > differ &&
              abs(bs_roll_cpy[i] - bs_roll_cpy[record_idx]) > differ) {
        bs_data_count[6] <- bs_data_count[6] + 1
      } 
      else if (abs(bs_pitch_cpy[i] - bs_pitch_cpy[record_idx]) > differ) {
        bs_data_count[1] <- bs_data_count[1] + 1
      } 
      else if (abs(bs_yaw_cpy[i] - bs_yaw_cpy[record_idx]) > differ) {
        bs_data_count[2] <- bs_data_count[2] + 1
      } 
      else {
        bs_data_count[3] <- bs_data_count[3] + 1
      }
      bs_valid_data_length <- bs_valid_data_length + 1
      bs_pitch_data[j] <- bs_pitch_cpy[j]
      bs_yaw_data[j] <- bs_yaw_cpy[j]
      bs_roll_data[j] <- bs_roll_cpy[j]
      j <- j + 1
      record_idx <- i
    }
  }
    i <- i + 1
}

##################################################################################################

## PLAN 2 (Abandoned)
## Method description: Data Processing: The original dataset contains a large number of 
##  data points with angles exceeding 180 degrees, with many even approaching 
##  360 degrees. It is my judgment that these data points are likely measurements 
##  from the negative coordinate axis by OptiTrack. This is because, under normal 
##  circumstances, the range for pitch, yaw, and roll should be between -180 and 180 
##  degrees. Therefore, the first step is to process the data such that any value 
##  exceeding 180 degrees is reduced by 360 degrees to obtain the correct rotational 
##  coordinate angle.
## The most crucial aspect is determining what constitutes a head turn. 
##  I have adopted the threshold defined by OptiTrack Prime^x 13, which 
##  indicates that each measurement may have a maximum error of 0.5 degrees. 
##  If a segment of data changes its trend (i.e., the sign of the difference 
##  between consecutive data points changes), we compare this segment to the 
##  threshold. If the change exceeds the threshold, it is considered a valid 
##  head turn. Therefore, the starting point of a head turn is the first value, 
##  and the ending point is when the sign of the value changes.
## This description pertains to determining rotation in a single direction. 
##  When considering combinations of rotations in the pitch, yaw, and roll directions, 
##  we need to determine the minimum change across the three directions and identify 
##  which directions' movements are valid within this smallest segment. There are eight 
##  possible combinations:
## 1. Pure pitch
## 2. Pure roll
## 3. Pure yaw
## 4. Pitch + roll
## 5. Pitch + yaw
## 6. Roll + yaw
## 7. Pitch + yaw + roll
## 8. No motion
## These eight combinations are recorded in the data_count vector. 
##  Whenever a movement is determined to fall under any of these categories, 
##  the angle change is recorded into the corresponding filtered vector.

## Implementation:

## count_same_trend counts the number of data in the same trend and
##  determines the length and returns it.
count_same_trend <- function(data_vec, idx, valid_length) {
  same_trend_num <- 0
  up_trend <- ifelse(data_vec[idx] <= data_vec[idx + 1], TRUE, FALSE)
  
  while (idx < valid_length) {
    ## Debug Usage
    ##if (abs(data_vec[idx + 1] - data_vec[idx] > 180)) {
    ##  print(paste("last idx:", idx + 1))
    ##  print(paste("last val:", data_vec[idx + 1]))
    ##  print(paste("next idx:", idx))
    ##  print(paste("next val:", data_vec[idx]))
    ##}
    if (up_trend && (data_vec[idx] > data_vec[idx + 1]) ||
        !up_trend && (data_vec[idx] <= data_vec[idx + 1])) {
      break
    } else {
      same_trend_num <- same_trend_num + 1
      idx <- idx + 1
    }
  }
  return(same_trend_num)
}

## Check whether it is a valid head turn and return a list that includes 
##  a boolean and the rotation angle.
check_valid_rotation <- function(data_vec, idx, threshold, valid_length) {
  data_sum <- 0
  up_trend <- ifelse(data_vec[idx] <= data_vec[idx + 1], TRUE, FALSE)
  rotation_data <- list(FALSE, 0)
  while (idx < valid_length) {
    ## Debug Usage
    ##if (abs(data_vec[idx + 1] - data_vec[idx] > 180)) {
    ##  print(paste("last idx:", idx + 1))
    ##  print(paste("last val:", data_vec[idx + 1]))
    ##  print(paste("next idx:", idx))
    ##  print(paste("next val:", data_vec[idx]))
    ##}
    if (up_trend && (data_vec[idx] > data_vec[idx + 1]) ||
        !up_trend && (data_vec[idx] <= data_vec[idx + 1])) {
      break
    } else {
      if (abs(data_vec[idx + 1] - data_vec[idx]) > 180) {
        data_sum <- data_sum + 180 - abs(data_vec[idx]) + 180 - abs(data_vec[idx + 1])
      }
      else {
        data_sum <- data_sum + abs(data_vec[idx + 1] - data_vec[idx])
      }
      idx <- idx + 1
    }
  }
  if (data_sum > threshold) {
    rotation_data[[1]] <- TRUE
    rotation_data[[2]] <- data_sum
  }
  return(rotation_data)
}


## handle_outliers deals with the data that's greater than 180 or smaller than -180,
##  and returns the list contains pitch_vec, yaw_vec and roll_vec in this order.
handle_outliers <- function(pitch_vec, yaw_vec, roll_vec, valid_row, valid_length) {
  
  for (i in valid_row:valid_length) {
    if (pitch_vec[i] > 180) {
      pitch_vec[i] <- pitch_vec[i] - 360
    } else if (pitch_vec[i] < -180) {
      pitch_vec[i] <- pitch_vec[i] + 360
    }
    if (yaw_vec[i] > 180) {
      yaw_vec[i] <- yaw_vec[i] - 360
    } else if (yaw_vec[i] < -180) {
      yaw_vec[i] <- yaw_vec[i] + 360
    }
    if (roll_vec[i] > 180) {
      roll_vec[i] <- roll_vec[i] - 360
    } else if (roll_vec[i] < -180) {
      roll_vec[i] <- roll_vec[i] + 360
    }
  } 
  cpy_list <- list(pitch_vec, yaw_vec, roll_vec)
  return(cpy_list)
}

## data_filter_counter Function: This function will count the combinations 
##  of rotations in the pitch, yaw, and roll directions, compiling them into 
##  the data_count vector. It will also record the angle changes of each valid 
##  head turn into the corresponding filtered vector. The function ultimately 
##  returns a list containing the four pieces of information described below:

##  index 1 contains a vector that includes the counting results
##  of different head rotation combinations;
##  index 2 contains the vector of filtered pitch data;
##  index 3 contains the vector of filtered yaw data;
##  index 4 contains the vector of filtered roll data;
data_filter_counter <- function(pitch_data_vec, yaw_data_vec, roll_data_vec, 
                                current_idx, valid_length, 
                                valid_threshold) {
  data_count <- c(0, 0, 0, 0, 0 ,0 ,0, 0)
  pitch_filtered_data <- numeric()
  yaw_filtered_data <- numeric()
  roll_filtered_data <- numeric()
  
  ## Handle the absolute angles > 180 degrees to the corrected range
  
  vec_list <- handle_outliers(pitch_data_vec, yaw_data_vec, roll_data_vec, 
                              current_idx, valid_length)
  pitch_data_vec <- vec_list[[1]]
  yaw_data_vec <- vec_list[[2]]
  roll_data_vec <- vec_list[[3]]
  
  ## Debug Usage
  ##for (i in current_idx:valid_length) {
  ##  if (abs(pitch_data_vec[i]) > 180) {
  ##    print(paste("pitch idx:", i, "value:", pitch_data_vec[i]))
  ##  } 
  ##  if (abs(yaw_data_vec[i]) > 180) {
  ##    print(paste("yaw idx:", i, "value:", yaw_data_vec[i]))
  ##  }
  ##  if (abs(roll_data_vec[i]) > 180) {
  ##    print(paste("roll idx:", i, "value:", roll_data_vec[i]))
  ##  }
  ##}
  
  ## Debug Usage
  ##if (any(abs(pitch_data_vec) > 180) || 
  ##    any(abs(yaw_data_vec) > 180) || 
  ##    any(abs(roll_data_vec) > 180)) {
  ## stop("Data still contains values exceeding 180 degrees after handling outliers. Front")
  ##}
  
  while (current_idx < valid_length) {
    
    ## Number of elements in the same trend
    pitch_trend_num <- count_same_trend(pitch_data_vec, current_idx, valid_length)
    yaw_trend_num <- count_same_trend(yaw_data_vec, current_idx, valid_length)
    roll_trend_num <- count_same_trend(roll_data_vec, current_idx, valid_length)
    min_trend_num <- min(pitch_trend_num, yaw_trend_num, roll_trend_num)
    interval <- min_trend_num + current_idx
    pitch_turned <- check_valid_rotation(pitch_data_vec, 
                                         current_idx, 
                                         valid_threshold, 
                                         interval)
    yaw_turned <- check_valid_rotation(yaw_data_vec, 
                                       current_idx, 
                                       valid_threshold, 
                                       interval)
    roll_turned <- check_valid_rotation(roll_data_vec, 
                                        current_idx, 
                                        valid_threshold, 
                                        interval)
    ## 1 for pitch; 2 for yaw; 3 for roll; 
    ## 4 for pitch + yaw; 5 for pitch + roll;
    ## 6 for yaw + roll; 7 for pitch + yaw + roll
    ## 8 for no motion
    if (pitch_turned[[1]] || yaw_turned[[1]] || roll_turned[[1]]) {
      if (pitch_turned[[1]] && yaw_turned[[1]] && roll_turned[[1]]) {
        data_count[7] <- data_count[7] + 1
        pitch_filtered_data <- c(pitch_filtered_data, pitch_turned[[2]])
        yaw_filtered_data <- c(yaw_filtered_data, yaw_turned[[2]])
        roll_filtered_data <- c(roll_filtered_data, roll_turned[[2]])
      }
      else if (pitch_turned[[1]] && yaw_turned[[1]]) {
        data_count[4] <- data_count[4] + 1
        pitch_filtered_data <- c(pitch_filtered_data, pitch_turned[[2]])
        yaw_filtered_data <- c(yaw_filtered_data, yaw_turned[[2]])
      }
      else if (pitch_turned[[1]] && roll_turned[[1]]) {
        data_count[5] <- data_count[5] + 1
        pitch_filtered_data <- c(pitch_filtered_data, pitch_turned[[2]])
        roll_filtered_data <- c(roll_filtered_data, roll_turned[[2]])
      }
      else if (yaw_turned[[1]] && roll_turned[[1]]) {
        data_count[6] <- data_count[6] + 1
        yaw_filtered_data <- c(yaw_filtered_data, yaw_turned[[2]])
        roll_filtered_data <- c(roll_filtered_data, roll_turned[[2]])
      }
      else if (pitch_turned[[1]]) {
        data_count[1] <- data_count[1] + 1
        pitch_filtered_data <- c(pitch_filtered_data, pitch_turned[[2]])
      }
      else if (yaw_turned[[1]]) {
        data_count[2] <- data_count[2] + 1
        yaw_filtered_data <- c(yaw_filtered_data, yaw_turned[[2]])
      }
      else {
        data_count[3] <- data_count[3] + 1
        roll_filtered_data <- c(roll_filtered_data, roll_turned[[2]])
      }
    }
    else {
      data_count[8] <- data_count[8] + 1
    }
    current_idx <- current_idx + min_trend_num
  }
  ## Debug Usage
  ##if (any(abs(pitch_filtered_data) > 180) || 
  ##    any(abs(yaw_filtered_data) > 180) || 
  ##    any(abs(roll_filtered_data) > 180)) {
  ##  stop("Data still contains values exceeding 180 degrees after handling outliers. Back")
  ##}
  pack_list <- list(data_count, pitch_filtered_data, yaw_filtered_data, roll_filtered_data)
  return(pack_list)
}


##################################################################################################

## PLAN 3 (Using)
## Method description: Data Processing: The original dataset contains a large number of 
##  data points with angles exceeding 180 degrees, with many even approaching 
##  360 degrees. It is my judgment that these data points are likely measurements 
##  from the negative coordinate axis by OptiTrack. This is because, under normal 
##  circumstances, the range for pitch, yaw, and roll should be between -180 and 180 
##  degrees. Therefore, the first step is to process the data such that any value 
##  exceeding 180 degrees is reduced by 360 degrees to obtain the correct rotational 
##  coordinate angle.
## The most crucial aspect is determining what constitutes a head turn. 
##  We have adopted the threshold defined by OptiTrack Prime^x 13, which 
##  indicates that each measurement may have a maximum error of 0.5 degrees. 
##  If a segment of data changes its trend (i.e., the sign of the difference 
##  between consecutive data points changes), we compare this segment to the 
##  threshold. If the change exceeds the threshold, it is considered a valid 
##  head turn. Therefore, the starting point of a head turn is the first value, 
##  and the ending point is when the sign of the value changes. But we can also
##  change the threshold depends on the real data results.
## This description pertains to determining rotation in a single direction. 
##  When considering combinations of rotations in the pitch, yaw, and roll directions, 
##  we need to determine the minimum change across the three directions and identify 
##  which directions' movements are valid within this smallest segment. There are eight 
##  possible combinations:
## 1. Pure pitch
## 2. Pure roll
## 3. Pure yaw
## 4. Pitch + roll
## 5. Pitch + yaw
## 6. Roll + yaw
## 7. Pitch + yaw + roll
## 8. No motion

## These eight combinations are recorded in the data_count vector.
##  Whenever a movement is determined to fall under any of these categories,
##  the angle change is recorded into the corresponding filtered vector.
##  The differences between this, plan 3, and plan 2 are:
## In plan 2, we directly choose the smallest change as the base,
##  and cut the larger changes to do the same comparison as the
##  smallest change. This causes a problem if we move our head
##  at a slower rate than a shorter but faster rotation in another
##  direction. The previous rotation might be counted as "no motion,"
##  leading to miscounting problems.
## In this plan, we divide the smallest changes and the larger
##  changes, and we determine the change with the smallest ending
##  index as the smallest change. Each time we find the shortest movement,
##  we check whether or not it is a valid motion. If it is, we count it;
##  if it is not, we delete it immediately.
## Then, we go to the list with longer rotations. If it is a valid rotation
##  with the given ending index, which is the shortest change's index,
##  we count it. Unlike how we deal with the shortest change, we don't
##  delete it if it isn't a valid rotation. Instead, we keep it until it becomes
##  the shortest change.

## Implementation:

## count_same_trend counts the number of data in the same trend and
##  determines the length and returns it.
##count_same_trend <- function(data_list, idx, valid_length) {
##  same_trend_num <- 0
##  up_trend <- ifelse(data_list[[idx]] <= data_list[[idx + 1]], TRUE, FALSE)
  
##  while (idx < valid_length) {
    ## Debug Usage
    ##if (abs(data_list[idx + 1] - data_list[idx] > 180)) {
    ##  print(paste("last idx:", idx + 1))
    ##  print(paste("last val:", data_list[idx + 1]))
    ##  print(paste("next idx:", idx))
    ##  print(paste("next val:", data_list[idx]))
    ##}
##    if (up_trend && (data_list[[idx]] > data_list[[idx + 1]]) ||
##        !up_trend && (data_list[[idx]] <= data_list[[idx + 1]])) {
##      break
##    } else {
##      same_trend_num <- same_trend_num + 1
##      idx <- idx + 1
##    }
##  }
##  return(same_trend_num)
##}

## count_valid_rotation counts whether it is a valid head turn
##   and return a list that includes a boolean and the rotation angle.
count_valid_rotation <- function(data_list, idx, threshold, valid_length) {
  start_idx <- idx
  data_sum <- 0
  up_trend <- ifelse(data_list[[idx]] <= data_list[[idx + 1]], TRUE, FALSE)
  ## rotation_data includes the data of:
  ## 1) boolean of whether or not it is a valid rotation
  ## 2) numeric of the angles it change
  ## 3) integer of starting index of the change
  ## 4) integer of end index of the change
  rotation_data <- list(FALSE, 0, start_idx, 0)
  
  while (idx < valid_length) {
    ## Debug Usage
    ##if (abs(data_list[idx + 1] - data_list[idx] > 180)) {
    ##  print(paste("last idx:", idx + 1))
    ##  print(paste("last val:", data_list[idx + 1]))
    ##  print(paste("next idx:", idx))
    ##  print(paste("next val:", data_list[idx]))
    ##}
    if (up_trend && (data_list[[idx]] > data_list[[idx + 1]]) ||
        !up_trend && (data_list[[idx]] <= data_list[[idx + 1]])) {
      break
    } else {
      if (abs(data_list[[idx + 1]] - data_list[[idx]]) > 180) {
        data_sum <- data_sum + 180 - abs(data_list[[idx]]) + 180 - abs(data_list[[idx + 1]])
      }
      else {
        data_sum <- data_sum + abs(data_list[[idx + 1]] - data_list[[idx]])
      }
      idx <- idx + 1
    }
  }
  if (data_sum > threshold) {
    rotation_data[[1]] <- TRUE
    rotation_data[[2]] <- data_sum
  }
  rotation_data[[4]] <- idx
  return(rotation_data)
}


## handle_outliers deals with the data that's greater than 180 or smaller than -180,
##  and returns the list contains pitch_vec, yaw_vec and roll_vec in this order.
handle_outliers <- function(pitch_list, yaw_list, roll_list, valid_row, valid_length) {
  
  for (i in valid_row:valid_length) {
    if (pitch_list[[i]] > 180) {
      pitch_list[[i]] <- pitch_list[[i]] - 360
    } else if (pitch_list[[i]] < -180) {
      pitch_list[[i]] <- pitch_list[[i]] + 360
    }
    if (yaw_list[[i]] > 180) {
      yaw_list[[i]] <- yaw_list[[i]] - 360
    } else if (yaw_list[i] < -180) {
      yaw_list[[i]] <- yaw_list[[i]] + 360
    }
    if (roll_list[[i]] > 180) {
      roll_list[[i]] <- roll_list[[i]] - 360
    } else if (roll_list[[i]] < -180) {
      roll_list[[i]] <- roll_list[[i]] + 360
    }
  } 
  cpy_list <- list(pitch_list, yaw_list, roll_list)
  return(cpy_list)
}

rotation_counter <- function(is_pitch_turned, is_yaw_turned, is_roll_turned, data_count) {
  ## 1 for pitch; 2 for yaw; 3 for roll; 
  ## 4 for pitch + yaw; 5 for pitch + roll;
  ## 6 for yaw + roll; 7 for pitch + yaw + roll
  ## 8 for no motion
  if (is_pitch_turned || is_yaw_turned || is_roll_turned) {
    if (is_pitch_turned && is_yaw_turned && is_roll_turned) {
      data_count[[7]] <- data_count[[7]] + 1
    }
    else if (is_pitch_turned && is_yaw_turned) {
      data_count[[4]] <- data_count[[4]] + 1
    }
    else if (is_pitch_turned && is_roll_turned) {
      data_count[[5]] <- data_count[[5]] + 1
    }
    else if (is_yaw_turned && is_roll_turned) {
      data_count[[6]] <- data_count[[6]] + 1
    }
    else if (is_pitch_turned) {
      data_count[[1]] <- data_count[[1]] + 1
    }
    else if (is_yaw_turned) {
      data_count[[2]] <- data_count[[2]] + 1
    }
    else {
      data_count[[3]] <- data_count[[3]] + 1
    }
  }
  else {
    data_count[[8]] <- data_count[[8]] + 1
  }
  return(data_count)
}

##split_and_count <- function(raw_data, start_idx, split_idx, end_idx, threshold) {
##  first_half <- raw_data[start:split_idx]
##  last_half <- raw_data[split_index:end_idx]
  
  ## count_valid_rotation <- function(data_list, idx, threshold, valid_length) 
##  first_half_result <- count_valid_rotation(first_half, start_idx, threshold, split_idx)
##  last_half_result <- count_valid_rotation(last_half, split_idx, end_idx)
##  result_vec <- c(first_half_result, last_half_result)
##  return(result_vec)
##}


## data_filter_counter Function: This function will count the combinations 
##  of rotations in the pitch, yaw, and roll directions, compiling them into 
##  the data_count vector. It will also record the angle changes of each valid 
##  head turn into the corresponding filtered vector. The function ultimately 
##  returns a list containing the four pieces of information described below:

##  index 1 contains a vector that includes the counting results
##  of different head rotation combinations;
##  index 2 contains the vector of filtered pitch data;
##  index 3 contains the vector of filtered yaw data;
##  index 4 contains the vector of filtered roll data;
data_filter_counter <- function(pitch_data_vec, yaw_data_vec, roll_data_vec, 
                                current_idx, valid_length, 
                                valid_threshold) {
  data_count <- list(0, 0, 0, 0, 0 ,0 ,0, 0)
  
  ## turning_data contains all the rotation angle data.
  pitch_turning_data <- list()
  yaw_turning_data <- list()
  roll_turning_data <- list()
  
  ## filtered_data contains all the valid rotation angle data.
  pitch_filtered_data <- list()
  yaw_filtered_data <- list()
  roll_filtered_data <- list()
  
  pitch_data_list <- list()
  yaw_data_list <- list()
  roll_data_list <- list()
  
  current_pitch_idx <- current_idx
  current_yaw_idx <- current_idx
  current_roll_idx <- current_idx
  
  for (i in 1:valid_length) {
    pitch_data_list <- append(pitch_data_list, pitch_data_vec[i])
    yaw_data_list <- append(yaw_data_list, yaw_data_vec[i])
    roll_data_list <- append(roll_data_list, roll_data_vec[i])
  }
  ## Handle the absolute angles > 180 degrees to the corrected range
  tempt_list <- handle_outliers(pitch_data_list, yaw_data_list, roll_data_list, 
                              current_idx, valid_length)
  pitch_data_list <- tempt_list[[1]]
  yaw_data_list <- tempt_list[[2]]
  roll_data_list <- tempt_list[[3]]
  
  
  tempt_idx <- current_idx
  while (tempt_idx < valid_length) {
    new_data <- count_valid_rotation(pitch_data_list, tempt_idx, valid_threshold, valid_length)
    pitch_turning_data <- append(pitch_turning_data, list(new_data))
    
    tempt_idx <- new_data[[4]]
  }
  
  tempt_idx <- current_idx
  while (tempt_idx < valid_length) {
    new_data <- count_valid_rotation(yaw_data_list, tempt_idx, valid_threshold, valid_length)
    yaw_turning_data <- append(yaw_turning_data, list(new_data))
    
    tempt_idx <- new_data[[4]]
  }
  
  tempt_idx <- current_idx
  while (tempt_idx < valid_length) {
    new_data <- count_valid_rotation(roll_data_list, tempt_idx, valid_threshold, valid_length)
    roll_turning_data <- append(roll_turning_data, list(new_data))
    
    tempt_idx <- new_data[[4]]
  }
  ##print(pitch_turning_data)
  ##print(yaw_turning_data)
  ##print(roll_turning_data)
  
  max_idx <- max(pitch_turning_data[[1]][[4]], yaw_turning_data[[1]][[4]], roll_turning_data[[1]][[4]])
  min_idx <- min(pitch_turning_data[[1]][[4]], yaw_turning_data[[1]][[4]], roll_turning_data[[1]][[4]])
  last_round <- FALSE
  while (max_idx <= valid_length) {
    tempt_bool_pitch <- FALSE
    tempt_bool_yaw <- FALSE
    tempt_bool_roll <- FALSE
    if (max_idx == min_idx) {
      if (pitch_turning_data[[1]][[1]]) {
        tempt_bool_pitch <- TRUE
        pitch_filtered_data <- append(pitch_filtered_data, pitch_turning_data[[1]][[2]])
      }
      if (yaw_turning_data[[1]][[1]]) {
        tempt_bool_yaw <- TRUE
        yaw_filtered_data <- append(yaw_filtered_data, yaw_turning_data[[1]][[2]])
      }
      if (roll_turning_data[[1]][[1]]) {
        tempt_bool_roll <- TRUE
        roll_filtered_data <- append(roll_filtered_data, roll_turning_data[[1]][[2]])
      }
      current_pitch_idx <- min_idx
      current_yaw_idx <- min_idx
      current_roll_idx <- min_idx
      
      pitch_turning_data <- pitch_turning_data[-1]
      yaw_turning_data <- yaw_turning_data[-1]
      roll_turning_data <- roll_turning_data[-1]
    }
    else {
      ## smallest_list contains the direction with the smallest ending index.
      smallest_list <- list()
      larger_list <- list()
      
      ## data_list contains the original raw data.
      smallest_data_list <- list()
      larger_data_list <- list()
      
      if (pitch_turning_data[[1]][[4]] > min_idx) {
        larger_list$pitch <- pitch_turning_data
        larger_data_list$pitch <- pitch_data_list
      } 
      else if (pitch_turning_data[[1]][[4]] == min_idx) {
        smallest_list$pitch <- pitch_turning_data
        smallest_data_list$pitch <- pitch_data_list
      }
      if (yaw_turning_data[[1]][[4]] > min_idx) {
        larger_list$yaw <- yaw_turning_data
        larger_data_list$yaw <- yaw_data_list
      }
      else if (yaw_turning_data[[1]][[4]] == min_idx) {
        smallest_list$yaw <- yaw_turning_data
        smallest_data_list$yaw <- yaw_data_list
      }
      if (roll_turning_data[[1]][[4]] > min_idx) {
        larger_list$roll <- roll_turning_data
        larger_data_list$roll <- roll_data_list
      }
      else if (roll_turning_data[[1]][[4]] == min_idx) {
        smallest_list$roll <- roll_turning_data
        smallest_data_list$roll <- roll_data_list
      }
      
      for (i in 1:length(smallest_list)) {
        if (smallest_list[[i]][[1]][[1]]) {
          if (names(smallest_data_list)[i] == "pitch") {
            ##print("counting smaller pitch")
            tempt_bool_pitch <- TRUE
            pitch_filtered_data <- append(pitch_filtered_data, pitch_turning_data[[1]][[2]])
            current_pitch_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            pitch_turning_data <- smallest_list[[i]]
          }
          else if (names(smallest_data_list)[i] == "yaw") {
            ##print("counting smaller yaw")
            tempt_bool_yaw <- TRUE
            yaw_filtered_data <- append(yaw_filtered_data, yaw_turning_data[[1]][[2]])
            current_yaw_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            yaw_turning_data <- smallest_list[[i]]
          }
          else if (names(smallest_data_list)[i] == "roll") {
            ##print("counting smaller roll")
            tempt_bool_roll <- TRUE
            roll_filtered_data <- append(roll_filtered_data, roll_turning_data[[1]][[2]])
            current_roll_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            roll_turning_data <- smallest_list[[i]]
          }

        }
        else {
          if (names(smallest_data_list)[i] == "pitch") {
            ##print("counting smaller pitch")
            current_pitch_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            pitch_turning_data <- smallest_list[[i]]
          }
          else if (names(smallest_data_list)[i] == "yaw") {
            ##print("counting smaller yaw")
            current_yaw_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            yaw_turning_data <- smallest_list[[i]]
          }
          else if (names(smallest_data_list)[i] == "roll") {
            ##print("counting smaller roll")
            current_roll_idx <- min_idx
            smallest_list[[i]] <- smallest_list[[i]][-1]
            roll_turning_data <- smallest_list[[i]]            
          }
        }
      }
      
      ## We split the rotation with the same ending index as the smallest
      ##  change, and determine whether or not it is , or it is still a 
      ##  valid rotation, if it is, we split it, if it is not, we keep it.
      for (j in 1:length(larger_list)) {
        ## count_valid_rotation <- function(data_list, idx, threshold, valid_length)
        ## rotation_data includes the data of:
        ## 1) boolean of whether or not it is a valid rotation
        ## 2) numeric of the angles it change
        ## 3) integer of starting index of the change
        ## 4) integer of end index of the change
        
        if (names(larger_data_list)[j] == "pitch") {
          tempt_rotation_result <- count_valid_rotation(larger_data_list[[j]], 
                                                        current_pitch_idx,
                                                        valid_threshold, 
                                                        min_idx)
          ##print("counting larger pitch")
          if (tempt_rotation_result[[1]]) {
            
            tempt_bool_pitch <- TRUE
            pitch_filtered_data <- append(pitch_filtered_data, tempt_rotation_result[[2]])
            pitch_turning_data[[1]] <- count_valid_rotation(larger_data_list[[j]], 
                                                            min_idx,
                                                            valid_threshold,
                                                            larger_list[[j]][[1]][[4]])
            current_pitch_idx <- min_idx
          }
        }

        else if (names(larger_data_list)[j] == "yaw") {
          tempt_rotation_result <- count_valid_rotation(larger_data_list[[j]], 
                                                        current_yaw_idx,
                                                        valid_threshold, 
                                                        min_idx)
          ##print("counting larger yaw")
          if (tempt_rotation_result[[1]]) {
            tempt_bool_yaw <- TRUE
            yaw_filtered_data <- append(yaw_filtered_data, tempt_rotation_result[[2]])
            yaw_turning_data[[1]] <- count_valid_rotation(larger_data_list[[j]], 
                                                          min_idx,
                                                          valid_threshold,
                                                          larger_list[[j]][[1]][[4]])
            current_yaw_idx <- min_idx
          }
            
        }
        else if (names(larger_data_list)[j] == "roll") {
          tempt_rotation_result <- count_valid_rotation(larger_data_list[[j]], 
                                                        current_roll_idx,
                                                        valid_threshold, 
                                                        min_idx)
          ##print("counting larger roll")
          if (tempt_rotation_result[[1]]) {
            tempt_bool_roll <- TRUE
            roll_filtered_data <- append(roll_filtered_data, tempt_rotation_result[[2]])
            roll_turning_data[[1]] <- count_valid_rotation(larger_data_list[[j]], 
                                                           min_idx,
                                                           valid_threshold,
                                                           larger_list[[j]][[1]][[4]])
            current_roll_idx <- min_idx
          }
        }
      }
      
    }

    data_count <- rotation_counter(tempt_bool_pitch, tempt_bool_yaw, 
                                   tempt_bool_roll, data_count)
    ##print(data_count)
    if (last_round) break
    
    max_idx <- max(pitch_turning_data[[1]][[4]], yaw_turning_data[[1]][[4]], roll_turning_data[[1]][[4]])
    min_idx <- min(pitch_turning_data[[1]][[4]], yaw_turning_data[[1]][[4]], roll_turning_data[[1]][[4]])
    ##print(paste("max_idx:", max_idx))
    ##print(paste("min_idx:", min_idx))
    if (min_idx == valid_length) {
      last_round <- TRUE
    }
  }
  ## Debug Usage
  ##if (any(abs(pitch_filtered_data) > 180) || 
  ##    any(abs(yaw_filtered_data) > 180) || 
  ##    any(abs(roll_filtered_data) > 180)) {
  ##  stop("Data still contains values exceeding 180 degrees after handling outliers. Back")
  ##}
  pack_list <- list(data_count, pitch_filtered_data, yaw_filtered_data, roll_filtered_data)
  return(pack_list)
}


##################################################################################################
## Plan 4 (Using)
## Instead of analysis the head rotation position and 
##  rotation angle of each turn, we choose to analysis
##  the position where the head stays.

## handle_outliers deals with the data that's greater than 180 or smaller than -180,
##  and returns the list contains pitch_vec, yaw_vec and roll_vec in this order.
handle_outliers <- function(pitch_vec, yaw_vec, roll_vec, valid_row, valid_length) {
  
  for (i in valid_row:valid_length) {
    ## pitch
    if (pitch_vec[i] > 180) {
      pitch_vec[i] <- pitch_vec[i] - 360
    } 
    else if (pitch_vec[i] < -180) {
      pitch_vec[i] <- pitch_vec[i] + 360
    }
    ## yaw
    if (yaw_vec[i] > 180) {
      yaw_vec[i] <- yaw_vec[i] - 360
    } 
    else if (yaw_vec[i] < -180) {
      yaw_vec[i] <- yaw_vec[i] + 360
    }
    ## roll
    if (roll_vec[i] > 180) {
      roll_vec[i] <- roll_vec[i] - 360
    } 
    else if (roll_vec[i] < -180) {
      roll_vec[i] <- roll_vec[i] + 360
    }
  } 
  cpy_list <- list(pitch_vec, yaw_vec, roll_vec)
  return(cpy_list)
}

rotation_counter <- function(is_pitch_turned, is_yaw_turned, is_roll_turned, data_count) {
  ## 1 for pitch; 2 for yaw; 3 for roll; 
  ## 4 for pitch + yaw; 5 for pitch + roll;
  ## 6 for yaw + roll; 7 for pitch + yaw + roll
  ## 8 for no motion
  if (is_pitch_turned || is_yaw_turned || is_roll_turned) {
    if (is_pitch_turned && is_yaw_turned && is_roll_turned) {
      data_count[[7]] <- data_count[[7]] + 1
    }
    else if (is_pitch_turned && is_yaw_turned) {
      data_count[[4]] <- data_count[[4]] + 1
    }
    else if (is_pitch_turned && is_roll_turned) {
      data_count[[5]] <- data_count[[5]] + 1
    }
    else if (is_yaw_turned && is_roll_turned) {
      data_count[[6]] <- data_count[[6]] + 1
    }
    else if (is_pitch_turned) {
      data_count[[1]] <- data_count[[1]] + 1
    }
    else if (is_yaw_turned) {
      data_count[[2]] <- data_count[[2]] + 1
    }
    else {
      data_count[[3]] <- data_count[[3]] + 1
    }
  }
  else {
    data_count[[8]] <- data_count[[8]] + 1
  }
  return(data_count)
}

determine_range <- function(position_vec, counting_list) {
  for (i in 1:length(position_vec)) {
    if (0 <= abs(position_vec[i]) && abs(position_vec[i]) <= 30) {
      counting_list[[1]] <- counting_list[[1]] + 1
    } else if (30 < abs(position_vec[i]) && abs(position_vec[i]) <= 60) {
      counting_list[[2]] <- counting_list[[2]] + 1
    } else if (60 < abs(position_vec[i]) && abs(position_vec[i]) <= 90) {
      counting_list[[3]] <- counting_list[[3]] + 1
    } else if (90 < abs(position_vec[i]) && abs(position_vec[i]) <= 120) {
      counting_list[[4]] <- counting_list[[4]] + 1
    } else if (120 < abs(position_vec[i]) && abs(position_vec[i]) <= 150) {
      counting_list[[5]] <- counting_list[[5]] + 1
    } else if (150 < abs(position_vec[i]) && abs(position_vec[i]) <= 180) {
      counting_list[[6]] <- counting_list[[6]] + 1
    } else {
      stop("determine_range, angle range > 180 degree.")
    }
  }
  return(counting_list)
}


position_counter <- function(position_list, valid_row, valid_length) {
  ## handle_outliers <- function(pitch_vec, yaw_vec, roll_vec, valid_row, valid_length)
  ## Index : Range
  ## 1: 0 - 30
  ## 2: 30 - 60
  ## 3: 60 - 90
  ## 4: 90 - 120
  ## 5: 120 - 150
  ## 6: 150 - 180
  position_list <- handle_outliers(position_list[[1]], position_list[[2]], position_list[[3]],
                                   valid_row, valid_length)
  pitch_count <- list(0,0,0,0,0,0)
  yaw_count <- list(0,0,0,0,0,0)
  roll_count <- list(0,0,0,0,0,0)
  
  for (i in 1:length(position_list)) {

    if (i == 1) {
      pitch_count <- determine_range(position_list[[i]], pitch_count)
    }
    else if (i == 2) {
      yaw_count <- determine_range(position_list[[i]], yaw_count)
    }
    else if (i == 3) {
      roll_count <- determine_range(position_list[[i]], roll_count)
    }
    else {
      stop("position_counter, length out of range 3.")
    }
  }
  
  return(list(pitch_count, yaw_count, roll_count))
}



##################################################################################################
## PART 2: INSTANCE
##################################################################################################

## APPs Names

## Beat Saber

## Initialize data
## Open our file (it should strictly align with our code, 
##  as some of the data is unique to this specific file).
f1 <- file.choose()
bs_data <- read.csv(f1)
valid_threshold <- 0.5

valid_row <- 2
bs_pitch_data <- bs_data$Pitch[valid_row:length(bs_data$Pitch)]
bs_yaw_data <- bs_data$Yaw[valid_row:length(bs_data$Yaw)]
bs_roll_data <- bs_data$Roll[valid_row:length(bs_data$Roll)]
bs_time <- bs_data$time[valid_row:length(bs_data$time)]
bs_valid_data_length <- length(bs_time)


## We make a copy of our data to show which data 
##  has been deleted and how much.
bs_pitch_cpy <- bs_pitch_data
bs_yaw_cpy <- bs_yaw_data
bs_roll_cpy <- bs_roll_data


## Incetance
valid_row <- 1

bs_pack_list <- position_counter(list(bs_pitch_cpy, bs_yaw_cpy, bs_roll_cpy), valid_row, bs_valid_data_length)




##bs_pack_list <- data_filter_counter(bs_pitch_cpy, 
##                                    bs_yaw_cpy, 
##                                    bs_roll_cpy, 
##                                    valid_row, 
##                                    bs_valid_data_length, 
##                                    valid_threshold)

##################################################################################################
## First Hand
f2 <- file.choose()
fh_data <- read.csv(f2)
valid_threshold <- 0.5
valid_row <- 1

fh_pitch_data <- fh_data$Pitch[valid_row:length(fh_data$Pitch)]
fh_yaw_data <- fh_data$Yaw[valid_row:length(fh_data$Yaw)]
fh_roll_data <- fh_data$Roll[valid_row:length(fh_data$Roll)]
fh_time <- fh_data$time[valid_row:length(fh_data$time)]
fh_valid_data_length <- length(fh_time)


## We make a copy of our data to show which data 
##  has been deleted and how much.
fh_pitch_cpy <- fh_pitch_data
fh_yaw_cpy <- fh_yaw_data
fh_roll_cpy <- fh_roll_data

# Instance
fh_pack_list <- position_counter(list(fh_pitch_cpy, fh_yaw_cpy, fh_roll_cpy), 
                                 valid_row, fh_valid_data_length)

##fh_pack_list <- data_filter_counter(fh_pitch_cpy, 
##                                    fh_yaw_cpy, 
##                                    fh_roll_cpy, 
##                                    valid_row, 
##                                    fh_valid_data_length, 
##                                    valid_threshold)
##################################################################################################
## Super Hot
f3 <- file.choose()
sh_data <- read.csv(f3)
valid_threshold <- 0.5
valid_row <- 1

sh_pitch_data <- sh_data$Pitch[valid_row:length(sh_data$Pitch)]
sh_yaw_data <- sh_data$Yaw[valid_row:length(sh_data$Yaw)]
sh_roll_data <- sh_data$Roll[valid_row:length(sh_data$Roll)]
sh_time <- sh_data$time[valid_row:length(sh_data$time)]
sh_valid_data_length <- length(sh_time)


## We make a copy of our data to show which data 
##  has been deleted and how much.
sh_pitch_cpy <- sh_pitch_data
sh_yaw_cpy <- sh_yaw_data
sh_roll_cpy <- sh_roll_data


# Instance
sh_pack_list <- position_counter(list(sh_pitch_cpy, sh_yaw_cpy, sh_roll_cpy), 
                                 valid_row, sh_valid_data_length)

##sh_pack_list <- data_filter_counter(sh_pitch_cpy, 
##                                    sh_yaw_cpy, 
##                                    sh_roll_cpy, 
##                                    valid_row, 
##                                    sh_valid_data_length, 
##                                    valid_threshold)

##################################################################################################
## EcoSphere dataset 1
## Comment: the reason of seperate EcoSphere into two parts is,
##  I tried to watch two different videos in this app so I want
##  to analysis them seperately.
f4 <- file.choose()
es1_data <- read.csv(f4)
valid_threshold <- 0.5
valid_row <- 1


es1_pitch_data <- es1_data$Pitch[valid_row:length(es1_data$Pitch)]
es1_yaw_data <- es1_data$Yaw[valid_row:length(es1_data$Yaw)]
es1_roll_data <- es1_data$Roll[valid_row:length(es1_data$Roll)]
es1_time <- es1_data$time[valid_row:length(es1_data$time)]
es1_valid_data_length <- length(es1_time)


## We make a copy of our data to show which data 
##  has been deleted and how much.
es1_pitch_cpy <- es1_pitch_data
es1_yaw_cpy <- es1_yaw_data
es1_roll_cpy <- es1_roll_data



# Instance
es1_pack_list <- position_counter(list(es1_pitch_cpy, es1_yaw_cpy, es1_roll_cpy), 
                                 valid_row, es1_valid_data_length)



##es1_pack_list <- data_filter_counter(es1_pitch_cpy, 
##                                    es1_yaw_cpy, 
##                                    es1_roll_cpy, 
##                                    valid_row, 
##                                    es1_valid_data_length, 
##                                    valid_threshold)

##################################################################################################
## EcoSphere dataset 2
f5 <- file.choose()
es2_data <- read.csv(f5)
valid_threshold <- 0.5
valid_row <- 1


es2_pitch_data <- es2_data$Pitch[valid_row:length(es2_data$Pitch)]
es2_yaw_data <- es2_data$Yaw[valid_row:length(es2_data$Yaw)]
es2_roll_data <- es2_data$Roll[valid_row:length(es2_data$Roll)]
es2_time <- es2_data$time[valid_row:length(es2_data$time)]
es2_valid_data_length <- length(es2_time)


## We make a copy of our data to show which data 
##  has been deleted and how much.
es2_pitch_cpy <- es2_pitch_data
es2_yaw_cpy <- es2_yaw_data
es2_roll_cpy <- es2_roll_data


# Instance
es2_pack_list <- position_counter(list(es2_pitch_cpy, es2_yaw_cpy, es2_roll_cpy), 
                                  valid_row, es2_valid_data_length)

##es2_pack_list <- data_filter_counter(es2_pitch_cpy, 
##                                    es2_yaw_cpy, 
##                                    es2_roll_cpy, 
##                                    valid_row, 
##                                    es2_valid_data_length, 
##                                    valid_threshold)

## Test
d1_test <- c(2,3,4,1,2,9)
d2_test <- c(-2,-3,-5,-7,-9,1)
d3_test <- c(-2,2,-2,2,-2,2)
valid_length_test <- 6
valid_row_test <- 1
valid_thres_test <- 2

test_pack_list <- data_filter_counter(d1_test, d2_test, d3_test, valid_row_test,
                                      valid_length_test, valid_thres_test)

##################################################################################################
## PART 3: Plotting
##################################################################################################

## TIME-BASED
# Stacked bar chart
# Assume apps is already defined
apps <- list(bs_pack_list, fh_pack_list, sh_pack_list, es1_pack_list, es2_pack_list)

# Create an empty data frame to store proportion data
data <- data.frame(
  App = character(),
  Action = character(),
  Proportion = numeric(),
  stringsAsFactors = FALSE
)

# Define the names of each app
app_names <- c("Beat Saber", "First Hand", "Super Hot", "EcoSphere1", "EcoSphere2")

# Iterate through each app, calculating the proportion of each action
for (i in 1:length(apps)) {
  app_data <- apps[[i]][[1]]
  app_data <- as.numeric(unlist(app_data))
  
  total_actions <- sum(app_data)
  
  
  # Add proportion data to the data frame
  data <- rbind(data, data.frame(App = app_names[i], Action = "None", Proportion = app_data[8] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Pitch", Proportion = app_data[1] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Yaw", Proportion = app_data[2] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Roll", Proportion = app_data[3] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Pitch+Yaw", Proportion = app_data[4] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Pitch+Roll", Proportion = app_data[5] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Yaw+Roll", Proportion = app_data[6] / total_actions))
  data <- rbind(data, data.frame(App = app_names[i], Action = "Pitch+Yaw+Roll", Proportion = app_data[7] / total_actions))
}

# Use ggplot2 to plot the stacked bar chart
library(ggplot2)

ggplot(data, aes(x = App, y = Proportion, fill = Action)) +
  geom_bar(stat = "identity") +
  scale_fill_manual(values = c("None" = "gray", 
                               "Pitch" = "blue", 
                               "Yaw" = "green", 
                               "Roll" = "red", 
                               "Pitch+Yaw" = "purple", 
                               "Pitch+Roll" = "orange", 
                               "Yaw+Roll" = "pink", 
                               "Pitch+Yaw+Roll" = "yellow")) + 
  labs(title = "Proportion of Head Movements in Different Apps",
       x = "App",
       y = "Proportion") +
  theme_minimal()


## Angle-based
bs_pack_list <- bs_pack_list_cpy
fh_pack_list <- fh_pack_list_cpy
sh_pack_list <- sh_pack_list_cpy
es1_pack_list <- es1_pack_list_cpy
es2_pack_list <- es2_pack_list_cpy

bs_new_pitch_list <- list()
bs_new_yaw_list <- list()
bs_new_roll_list <- list()

fh_new_pitch_list <- list()
fh_new_yaw_list <- list()
fh_new_roll_list <- list()

sh_new_pitch_list <- list()
sh_new_yaw_list <- list()
sh_new_roll_list <- list()

es1_new_pitch_list <- list()
es1_new_yaw_list <- list()
es1_new_roll_list <- list()

es2_new_pitch_list <- list()
es2_new_yaw_list <- list()
es2_new_roll_list <- list()

# Function to filter data
filter_data <- function(pack_list, new_pitch_list, new_yaw_list, new_roll_list, new_threshold) {
  for (i in 2:length(pack_list)) {
    if (i == 2) {
      for (j in 1:length(pack_list[[2]])) {
        if (pack_list[[i]][[j]] >= new_threshold) {
          new_pitch_list <- append(new_pitch_list, pack_list[[i]][[j]])
        }
      }
    } else if (i == 3) {
      for (j in 1:length(pack_list[[3]])) {
        if (pack_list[[i]][[j]] >= new_threshold) {
          new_yaw_list <- append(new_yaw_list, pack_list[[i]][[j]])
        }
      }
    } else if (i == 4) {
      for (j in 1:length(pack_list[[4]])) {
        if (pack_list[[i]][[j]] >= new_threshold) {
          new_roll_list <- append(new_roll_list, pack_list[[i]][[j]])
        }
      }
    }
  }
  return(list(new_pitch_list, new_yaw_list, new_roll_list))
}

new_threshold <- 5
bs_filtered <- filter_data(bs_pack_list, bs_new_pitch_list, bs_new_yaw_list, bs_new_roll_list, new_threshold)
fh_filtered <- filter_data(fh_pack_list, fh_new_pitch_list, fh_new_yaw_list, fh_new_roll_list, new_threshold)
sh_filtered <- filter_data(sh_pack_list, sh_new_pitch_list, sh_new_yaw_list, sh_new_roll_list, new_threshold)
es1_filtered <- filter_data(es1_pack_list, es1_new_pitch_list, es1_new_yaw_list, es1_new_roll_list, new_threshold)
es2_filtered <- filter_data(es2_pack_list, es2_new_pitch_list, es2_new_yaw_list, es2_new_roll_list, new_threshold)

bs_pack_list <- list(0, bs_filtered[[1]], bs_filtered[[2]], bs_filtered[[3]])
fh_pack_list <- list(0, fh_filtered[[1]], fh_filtered[[2]], fh_filtered[[3]])
sh_pack_list <- list(0, sh_filtered[[1]], sh_filtered[[2]], sh_filtered[[3]])
es1_pack_list <- list(0, es1_filtered[[1]], es1_filtered[[2]], es1_filtered[[3]])
es2_pack_list <- list(0, es2_filtered[[1]], es2_filtered[[2]], es2_filtered[[3]])

## Violin Plots
library(ggplot2)
library(dplyr)
library(tidyr)

apps <- list(bs_pack_list, fh_pack_list, sh_pack_list, es1_pack_list, es2_pack_list)
app_names <- c("Beat Saber", "First Hand", "Super Hot", "EcoSphere1", "EcoSphere2")

data <- data.frame(
  App = character(),
  Direction = character(),
  Value = numeric(),
  stringsAsFactors = FALSE
)

for (i in 1:length(apps)) {
  app_data <- apps[[i]]
  app_name <- app_names[i]
  
  pitch_data <- as.numeric(app_data[[2]])
  yaw_data <- as.numeric(app_data[[3]])
  roll_data <- as.numeric(app_data[[4]])
  
  data <- rbind(data, data.frame(App = rep(app_name, length(pitch_data)), Direction = rep("Pitch", length(pitch_data)), Value = pitch_data))
  data <- rbind(data, data.frame(App = rep(app_name, length(yaw_data)), Direction = rep("Yaw", length(yaw_data)), Value = yaw_data))
  data <- rbind(data, data.frame(App = rep(app_name, length(roll_data)), Direction = rep("Roll", length(roll_data)), Value = roll_data))
}

ggplot(data, aes(x = App, y = Value, fill = Direction)) +
  geom_violin(trim = FALSE) +
  scale_fill_manual(values = c("Pitch" = "red", "Yaw" = "blue", "Roll" = "green")) +
  theme_minimal() +
  labs(title = "Violin Plot of Pitch, Yaw, and Roll for Each Application",
       x = "Application",
       y = "Degree",
       fill = "Direction")




########################################
## Stacked Bar Chart (plan 4 only)

## Pitch
# Lists of data for each app
## pack_list : list(pitch_count, yaw_count, roll_count)
## _count : list(0,0,0,0,0,0,0)
apps <- list(bs_pack_list, fh_pack_list, sh_pack_list, es1_pack_list, es2_pack_list)

# Create an empty data frame to store the counts
data <- data.frame(
  App = character(),
  Range = character(),
  Proportion = numeric(),
  stringsAsFactors = FALSE
)

# Define the names of each app
app_names <- c("Beat Saber", "First Hand", "Super Hot", "EcoSphere1", "EcoSphere2")

# Iterate through each app, calculating the proportion of each action
for (i in 1:length(apps)) {
  app_data <- apps[[i]][[1]] 
  
  total_angles <- sum(unlist(app_data))
  
  if (total_angles == 0) {
    total_angles <- 1 
  }
  
  # Add proportion data to the data frame
  data <- rbind(data, data.frame(App = app_names[i], Range = "0-30", Proportion = app_data[[1]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "30-60", Proportion = app_data[[2]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "60-90", Proportion = app_data[[3]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "90-120", Proportion = app_data[[4]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "120-150", Proportion = app_data[[5]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "150-180", Proportion = app_data[[6]] / total_angles))
}

library(ggplot2)

data$Range <- factor(data$Range, levels = c("0-30", "30-60", "60-90", "90-120", "120-150", "150-180"))

ggplot(data, aes(x = App, y = Proportion, fill = Range)) +
  geom_bar(stat = "identity") +
  scale_fill_manual(values = c("0-30" = "gray", 
                               "30-60" = "blue", 
                               "60-90" = "green", 
                               "90-120" = "red", 
                               "120-150" = "purple", 
                               "150-180" = "orange")) + 
  labs(title = "Proportion of Head Movement in Different Angle Ranges \nin the Pitch Direction of Various Apps",
       x = "App",
       y = "Proportion") +
  theme_minimal()

########################################
## Yaw
apps <- list(bs_pack_list, fh_pack_list, sh_pack_list, es1_pack_list, es2_pack_list)

# Create an empty data frame to store the counts
data <- data.frame(
  App = character(),
  Range = character(),
  Proportion = numeric(),
  stringsAsFactors = FALSE
)

# Define the names of each app
app_names <- c("Beat Saber", "First Hand", "Super Hot", "EcoSphere1", "EcoSphere2")

# Iterate through each app, calculating the proportion of each action
for (i in 1:length(apps)) {
  app_data <- apps[[i]][[2]]
  
  total_angles <- sum(unlist(app_data))
  
  if (total_angles == 0) {
    total_angles <- 1 
  }
  
  # Add proportion data to the data frame
  data <- rbind(data, data.frame(App = app_names[i], Range = "0-30", Proportion = app_data[[1]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "30-60", Proportion = app_data[[2]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "60-90", Proportion = app_data[[3]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "90-120", Proportion = app_data[[4]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "120-150", Proportion = app_data[[5]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "150-180", Proportion = app_data[[6]] / total_angles))
}

# Use ggplot2 to plot the stacked bar chart for pitch direction
library(ggplot2)

data$Range <- factor(data$Range, levels = c("0-30", "30-60", "60-90", "90-120", "120-150", "150-180"))

ggplot(data, aes(x = App, y = Proportion, fill = Range)) +
  geom_bar(stat = "identity") +
  scale_fill_manual(values = c("0-30" = "gray", 
                               "30-60" = "blue", 
                               "60-90" = "green", 
                               "90-120" = "red", 
                               "120-150" = "purple", 
                               "150-180" = "orange")) + 
  labs(title = "Proportion of Head Movement in Different Angle Ranges in the Yaw Direction of Various Apps",
       x = "App",
       y = "Proportion") +
  theme_minimal()


########################################
## Roll
apps <- list(bs_pack_list, fh_pack_list, sh_pack_list, es1_pack_list, es2_pack_list)

# Create an empty data frame to store the counts
data <- data.frame(
  App = character(),
  Range = character(),
  Proportion = numeric(),
  stringsAsFactors = FALSE
)

# Define the names of each app
app_names <- c("Beat Saber", "First Hand", "Super Hot", "EcoSphere1", "EcoSphere2")

# Iterate through each app, calculating the proportion of each action
for (i in 1:length(apps)) {
  app_data <- apps[[i]][[3]] 
  
  total_angles <- sum(unlist(app_data))
  
  if (total_angles == 0) {
    total_angles <- 1 
  }
  
  # Add proportion data to the data frame
  data <- rbind(data, data.frame(App = app_names[i], Range = "0-30", Proportion = app_data[[1]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "30-60", Proportion = app_data[[2]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "60-90", Proportion = app_data[[3]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "90-120", Proportion = app_data[[4]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "120-150", Proportion = app_data[[5]] / total_angles))
  data <- rbind(data, data.frame(App = app_names[i], Range = "150-180", Proportion = app_data[[6]] / total_angles))
}

library(ggplot2)

data$Range <- factor(data$Range, levels = c("0-30", "30-60", "60-90", "90-120", "120-150", "150-180"))

ggplot(data, aes(x = App, y = Proportion, fill = Range)) +
  geom_bar(stat = "identity") +
  scale_fill_manual(values = c("0-30" = "gray", 
                               "30-60" = "blue", 
                               "60-90" = "green", 
                               "90-120" = "red", 
                               "120-150" = "purple", 
                               "150-180" = "orange")) + 
  labs(title = "Proportion of Head Movement in Different Angle Ranges \nin the Roll Direction of Various Apps",
       x = "App",
       y = "Proportion") +
  theme_minimal()
